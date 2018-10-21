using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Accelerators
{
  public class LinearBVHNode
  {
    public Bounds3D bounds;
    //union {
      public int primitivesOffset;   // leaf
      public int secondChildOffset;  // interior
    //};
    public int nPrimitives;  // 0 -> interior node
    public byte axis;          // interior node: xyz
    //byte pad[1];        // ensure 32 byte total size
  };

  public    enum SplitMethod { SAH, HLBVH, Middle, EqualCounts };

  public class BVHPrimitiveInfo
  {
    public BVHPrimitiveInfo() { }

    public BVHPrimitiveInfo(int primitiveNumber, Bounds3D bounds)
    {
      _primitiveNumber = primitiveNumber;
      _bounds = bounds;
      _centroid = 0.5 * bounds.MinPoint + .5f * bounds.MaxPoint;
    }
    public int _primitiveNumber;
    public Bounds3D _bounds;
    public Point3D _centroid;
  };

  public class BVHBuildNode
  {
    // BVHBuildNode Public Methods
    public void InitLeaf(int first, int n, Bounds3D b) {
      firstPrimOffset = first;
      nPrimitives = n;
      bounds = b;
      children[0] = children[1] = null;
      //++leafNodes;
      //++totalLeafNodes;
      //totalPrimitives += n;
    }
    public void InitInterior(int axis, BVHBuildNode c0, BVHBuildNode c1)
    {
      children[0] = c0;
      children[1] = c1;
      bounds = c0.bounds.Union(c1.bounds);
      splitAxis = axis;
      nPrimitives = 0;
      //++interiorNodes;
    }
    public Bounds3D bounds;
    public BVHBuildNode[] children = new BVHBuildNode[2];
    public int splitAxis, firstPrimOffset, nPrimitives;
  };


public class BvhAccelerator : Aggregate
  {
    private readonly int maxPrimsInNode;
    private readonly SplitMethod splitMethod;
    List<Primitive> primitives;
    List<LinearBVHNode> nodes = null;

    public static BvhAccelerator Create(IEnumerable<Primitive> prims, ParamSet paramSet)
    {
      throw new NotImplementedException();
    }

    public BvhAccelerator(IEnumerable<Primitive> p, int maxPrimsInNode = 1, SplitMethod splitMethod = SplitMethod.SAH)
    {
      this.maxPrimsInNode = Math.Min(255, maxPrimsInNode);
      this.splitMethod = splitMethod;
      primitives = p.ToList();

      //ProfilePhase _(Prof::AccelConstruction);
      if (!primitives.Any())
      {
        return;
      }
      // Build BVH from _primitives_

      // Initialize _primitiveInfo_ array for primitives
      List<BVHPrimitiveInfo> primitiveInfo = new List<BVHPrimitiveInfo>(primitives.Count);
      for (int i = 0; i < primitives.Count; ++i)
      {
        primitiveInfo[i] = new BVHPrimitiveInfo(i, primitives[i].WorldBound());
      }

      // Build BVH tree for primitives using _primitiveInfo_
      int totalNodes = 0;
      List<Primitive> orderedPrims = new List<Primitive>(primitives.Count);
      BVHBuildNode root;
      if (splitMethod == SplitMethod.HLBVH)
      {
        root = HLBVHBuild(primitiveInfo, out totalNodes, orderedPrims);
      }
      else
      {
        root = RecursiveBuild(primitiveInfo, 0, primitives.Count,
                              ref totalNodes, orderedPrims);
      }

      primitives = orderedPrims;
      //primitiveInfo.resize(0);
      //LOG(INFO) << StringPrintf("BVH created with %d nodes for %d primitives (%.2f MB), arena allocated %.2f MB",
      //totalNodes, (int)primitives.size(),
      //float(totalNodes * sizeof(LinearBVHNode)) /
      //  (1024.f * 1024.f),
      //float(arena.TotalAllocated()) /
      //  (1024.f * 1024.f));

      // Compute representation of depth-first traversal of BVH tree
      //treeBytes += totalNodes * sizeof(LinearBVHNode) + sizeof(*this) +
      //  primitives.size() * sizeof(primitives[0]);
      //nodes = new List<LinearBVHNode>(totalNodes);
      int offset = 0;
      flattenBVHTree(root, ref offset);
      //CHECK_EQ(totalNodes, offset);

    }

    public int flattenBVHTree(BVHBuildNode node, ref int offset)
    {
      LinearBVHNode linearNode = nodes[offset];
      linearNode.bounds = node.bounds;
      int myOffset = (offset)++;
      if (node.nPrimitives > 0)
      {
        //CHECK(!node->children[0] && !node->children[1]);
        //CHECK_LT(node->nPrimitives, 65536);
        linearNode.primitivesOffset = node.firstPrimOffset;
        linearNode.nPrimitives = node.nPrimitives;
      }
      else
      {
        // Create interior flattened BVH node
        linearNode.axis = Convert.ToByte(node.splitAxis);
        linearNode.nPrimitives = 0;
        flattenBVHTree(node.children[0], ref offset);
        linearNode.secondChildOffset =
          flattenBVHTree(node.children[1], ref offset);
      }
      return myOffset;
    }

    private BVHBuildNode RecursiveBuild(List<BVHPrimitiveInfo> primitiveInfo, int start, int end, ref int totalNodes, List<Primitive> orderedPrims)
    {
      //CHECK_NE(start, end);
      BVHBuildNode node = new BVHBuildNode();
      totalNodes++;
      // Compute bounds of all primitives in BVH node
      Bounds3D bounds = new Bounds3D();
      for (int i = start; i < end; ++i)
      {
        bounds = bounds.Union(primitiveInfo[i]._bounds);
      }

      int nPrimitives = end - start;
      if (nPrimitives == 1)
      {
        // Create leaf _BVHBuildNode_
        int firstPrimOffset = orderedPrims.Count;
        for (int i = start; i < end; ++i)
        {
          int primNum = primitiveInfo[i]._primitiveNumber;
          orderedPrims.Add(primitives[primNum]);
        }
        node.InitLeaf(firstPrimOffset, nPrimitives, bounds);
        return node;
      }
      else
      {
        // Compute bound of primitive centroids, choose split dimension _dim_
        Bounds3D centroidBounds = new Bounds3D();
        for (int i = start; i < end; ++i)
        {
          centroidBounds = centroidBounds.Union(primitiveInfo[i]._centroid);
        }

        int dim = centroidBounds.MaximumExtent;

        // Partition primitives into two sets and build children
        int mid = (start + end) / 2;
        if (centroidBounds.MaxPoint[dim] == centroidBounds.MinPoint[dim])
        {
          // Create leaf _BVHBuildNode_
          int firstPrimOffset = orderedPrims.Count;
          for (int i = start; i < end; ++i)
          {
            int primNum = primitiveInfo[i]._primitiveNumber;
            orderedPrims.Add(primitives[primNum]);
          }
          node.InitLeaf(firstPrimOffset, nPrimitives, bounds);
          return node;
        }
        else
        {
          // Partition primitives based on _splitMethod_

          // todo: implement me!!!
          //switch (splitMethod)
          //{
          //  case SplitMethod.Middle:
          //    {
          //      // Partition primitives through node's midpoint
          //      double pmid =
          //          (centroidBounds.MinPoint[dim] + centroidBounds.MaxPoint[dim]) / 2.0;
          //      BVHPrimitiveInfo* midPtr = std::partition(
          //          &primitiveInfo[start], &primitiveInfo[end - 1] + 1,
          //          [dim, pmid](const BVHPrimitiveInfo &pi) {
          //        return pi.centroid[dim] < pmid;
          //      });
          //      mid = midPtr - primitiveInfo[0];
          //      // For lots of prims with large overlapping bounding boxes, this
          //      // may fail to partition; in that case don't break and fall
          //      // through
          //      // to EqualCounts.
          //      if (mid != start && mid != end)
          //      {
          //        break;
          //      }
          //    }
          //  case SplitMethod.EqualCounts:
          //    {
          //      // Partition primitives into equally-sized subsets
          //      mid = (start + end) / 2;
          //      std::nth_element(&primitiveInfo[start], &primitiveInfo[mid],
          //                       &primitiveInfo[end - 1] + 1,
          //                       [dim](const BVHPrimitiveInfo &a,
          //                             const BVHPrimitiveInfo &b) {
          //        return a.centroid[dim] < b.centroid[dim];
          //      });
          //      break;
          //    }
          //  case SplitMethod.SAH:
          //  default:
          //    {
          //      // Partition primitives using approximate SAH
          //      if (nPrimitives <= 2)
          //      {
          //        // Partition primitives into equally-sized subsets
          //        mid = (start + end) / 2;
          //        std::nth_element(&primitiveInfo[start], &primitiveInfo[mid],
          //                         &primitiveInfo[end - 1] + 1,

          //                         [dim](const BVHPrimitiveInfo &a,
          //                                 const BVHPrimitiveInfo &b) {
          //          return a.centroid[dim] <
          //                 b.centroid[dim];
          //        });
          //      }
          //      else
          //      {
          //        // Allocate _BucketInfo_ for SAH partition buckets
          //        PBRT_CONSTEXPR int nBuckets = 12;
          //        BucketInfo buckets[nBuckets];

          //        // Initialize _BucketInfo_ for SAH partition buckets
          //        for (int i = start; i < end; ++i)
          //        {
          //          int b = nBuckets *
          //                  centroidBounds.Offset(
          //                      primitiveInfo[i].centroid)[dim];
          //          if (b == nBuckets) b = nBuckets - 1;
          //          CHECK_GE(b, 0);
          //          CHECK_LT(b, nBuckets);
          //          buckets[b].count++;
          //          buckets[b].bounds =
          //              Union(buckets[b].bounds, primitiveInfo[i].bounds);
          //        }

          //        // Compute costs for splitting after each bucket
          //        Float cost[nBuckets - 1];
          //        for (int i = 0; i < nBuckets - 1; ++i)
          //        {
          //          Bounds3f b0, b1;
          //          int count0 = 0, count1 = 0;
          //          for (int j = 0; j <= i; ++j)
          //          {
          //            b0 = Union(b0, buckets[j].bounds);
          //            count0 += buckets[j].count;
          //          }
          //          for (int j = i + 1; j < nBuckets; ++j)
          //          {
          //            b1 = Union(b1, buckets[j].bounds);
          //            count1 += buckets[j].count;
          //          }
          //          cost[i] = 1 +
          //                    (count0 * b0.SurfaceArea() +
          //                     count1 * b1.SurfaceArea()) /
          //                        bounds.SurfaceArea();
          //        }

          //        // Find bucket to split at that minimizes SAH metric
          //        Float minCost = cost[0];
          //        int minCostSplitBucket = 0;
          //        for (int i = 1; i < nBuckets - 1; ++i)
          //        {
          //          if (cost[i] < minCost)
          //          {
          //            minCost = cost[i];
          //            minCostSplitBucket = i;
          //          }
          //        }

          //        // Either create leaf or split primitives at selected SAH
          //        // bucket
          //        Float leafCost = nPrimitives;
          //        if (nPrimitives > maxPrimsInNode || minCost < leafCost)
          //        {
          //          BVHPrimitiveInfo* pmid = std::partition(
          //              &primitiveInfo[start], &primitiveInfo[end - 1] + 1,

          //              [=](const BVHPrimitiveInfo &pi) {
          //            int b = nBuckets *
          //                    centroidBounds.Offset(pi.centroid)[dim];
          //            if (b == nBuckets) b = nBuckets - 1;
          //            CHECK_GE(b, 0);
          //            CHECK_LT(b, nBuckets);
          //            return b <= minCostSplitBucket;
          //          });
          //          mid = pmid - &primitiveInfo[0];
          //        }
          //        else
          //        {
          //          // Create leaf _BVHBuildNode_
          //          int firstPrimOffset = orderedPrims.size();
          //          for (int i = start; i < end; ++i)
          //          {
          //            int primNum = primitiveInfo[i].primitiveNumber;
          //            orderedPrims.push_back(primitives[primNum]);
          //          }
          //          node->InitLeaf(firstPrimOffset, nPrimitives, bounds);
          //          return node;
          //        }
          //      }
          //      break;
          //    }
          //}
          node.InitInterior(dim,
                             RecursiveBuild(primitiveInfo, start, mid,
                                            ref totalNodes, orderedPrims),
                             RecursiveBuild(primitiveInfo, mid, end,
                                            ref totalNodes, orderedPrims));
        }
      }
      return node;
    }

    private BVHBuildNode HLBVHBuild(List<BVHPrimitiveInfo> primitiveInfo, out int totalNodes, List<Primitive> orderedPrims)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Bounds3D WorldBound()
    {
      return nodes != null ? nodes[0].bounds : new Bounds3D();
    }

    /// <inheritdoc />
    public override bool Intersect(Ray ray, out SurfaceInteraction isect)
    {
      isect = null;

      if (nodes == null)
      {
        return false;
      }

      //ProfilePhase p(Prof::AccelIntersect);
      bool hit = false;
      Vector3D invDir = new Vector3D(1.0 / ray.Direction.X, 1.0 / ray.Direction.Y, 1.0 / ray.Direction.Z);
      bool[] dirIsNeg = new bool[3] { invDir.X < 0, invDir.Y < 0, invDir.Z < 0 };
      // Follow ray through BVH nodes to find primitive intersections
      int toVisitOffset = 0, currentNodeIndex = 0;
      int[] nodesToVisit = new int[64];
      while (true)
      {
        LinearBVHNode node = nodes[currentNodeIndex];
        // Check ray against BVH node
        if (node.bounds.IntersectP(ray, invDir, dirIsNeg))
        {
          if (node.nPrimitives > 0)
          {
            // Intersect ray with primitives in leaf BVH node
            for (int i = 0; i < node.nPrimitives; ++i)
            {
              if (primitives[node.primitivesOffset + i].Intersect(
                ray, out isect))
              {
                hit = true;
              }
            }

            if (toVisitOffset == 0)
            {
              break;
            }

            currentNodeIndex = nodesToVisit[--toVisitOffset];
          }
          else
          {
            // Put far BVH node on _nodesToVisit_ stack, advance to near
            // node
            if (dirIsNeg[node.axis])
            {
              nodesToVisit[toVisitOffset++] = currentNodeIndex + 1;
              currentNodeIndex = node.secondChildOffset;
            }
            else
            {
              nodesToVisit[toVisitOffset++] = node.secondChildOffset;
              currentNodeIndex = currentNodeIndex + 1;
            }
          }
        }
        else
        {
          if (toVisitOffset == 0)
          {
            break;
          }

          currentNodeIndex = nodesToVisit[--toVisitOffset];
        }
      }
      return hit;
    }

    /// <inheritdoc />
    public override bool IntersectP(Ray ray)
    {
      if (nodes == null)
      {
        return false;
      }

      //ProfilePhase p(Prof::AccelIntersectP);
      Vector3D invDir = new Vector3D(1.0 / ray.Direction.X, 1.0 / ray.Direction.Y, 1.0 / ray.Direction.Z);
      bool[] dirIsNeg = new bool[3] { invDir.X < 0, invDir.Y < 0, invDir.Z < 0 };
      int[] nodesToVisit = new int[64];
      int toVisitOffset = 0, currentNodeIndex = 0;
      while (true)
      {
        LinearBVHNode node = nodes[currentNodeIndex];
        if (node.bounds.IntersectP(ray, invDir, dirIsNeg))
        {
          // Process BVH node _node_ for traversal
          if (node.nPrimitives > 0)
          {
            for (int i = 0; i < node.nPrimitives; ++i)
            {
              if (primitives[node.primitivesOffset + i].IntersectP(
                ray))
              {
                return true;
              }
            }
            if (toVisitOffset == 0)
            {
              break;
            }

            currentNodeIndex = nodesToVisit[--toVisitOffset];
          }
          else
          {
            if (dirIsNeg[node.axis])
            {
              /// second child first
              nodesToVisit[toVisitOffset++] = currentNodeIndex + 1;
              currentNodeIndex = node.secondChildOffset;
            }
            else
            {
              nodesToVisit[toVisitOffset++] = node.secondChildOffset;
              currentNodeIndex = currentNodeIndex + 1;
            }
          }
        }
        else
        {
          if (toVisitOffset == 0)
          {
            break;
          }

          currentNodeIndex = nodesToVisit[--toVisitOffset];
        }
      }
      return false;
    }
  }
}
