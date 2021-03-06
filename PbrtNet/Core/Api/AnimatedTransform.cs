﻿// -----------------------------------------------------------------------
// <copyright file="AnimatedTransform.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class AnimatedTransform
  {
    private readonly bool _actuallyAnimated;
    private readonly Transform _endTransform;

    private readonly Transform _startTransform;
    private readonly double _endTime;
    private readonly double _startTime;
    private bool _hasRotation;
    private Quaternion[] R = new Quaternion[2];
    private Matrix4x4[] S = new Matrix4x4[2];

    private Vector3D[] T = new Vector3D[2];

    public AnimatedTransform(Transform startTransform, double startTime, Transform endTransform, double endTime)
    {
      _startTransform = startTransform;
      _endTransform = endTransform;
      _startTime = startTime;
      _endTime = endTime;
      _actuallyAnimated = (startTransform != endTransform);
      if (!_actuallyAnimated)
      {
        return;
      }
        //Decompose(startTransform->m, &T[0], &R[0], &S[0]);
        //Decompose(endTransform->m, &T[1], &R[1], &S[1]);
        //// Flip _R[1]_ if needed to select shortest path
        //if (Dot(R[0], R[1]) < 0) R[1] = -R[1];
        //hasRotation = Dot(R[0], R[1]) < 0.9995f;
        //// Compute terms of motion derivative function
        //if (hasRotation)
        //{
        //  Float cosTheta = Dot(R[0], R[1]);
        //  Float theta = std::acos(Clamp(cosTheta, -1, 1));
        //  Quaternion qperp = Normalize(R[1] - R[0] * cosTheta);

        //  Float t0x = T[0].x;
        //  Float t0y = T[0].y;
        //  Float t0z = T[0].z;
        //  Float t1x = T[1].x;
        //  Float t1y = T[1].y;
        //  Float t1z = T[1].z;
        //  Float q1x = R[0].v.x;
        //  Float q1y = R[0].v.y;
        //  Float q1z = R[0].v.z;
        //  Float q1w = R[0].w;
        //  Float qperpx = qperp.v.x;
        //  Float qperpy = qperp.v.y;
        //  Float qperpz = qperp.v.z;
        //  Float qperpw = qperp.w;
        //  Float s000 = S[0].m[0][0];
        //  Float s001 = S[0].m[0][1];
        //  Float s002 = S[0].m[0][2];
        //  Float s010 = S[0].m[1][0];
        //  Float s011 = S[0].m[1][1];
        //  Float s012 = S[0].m[1][2];
        //  Float s020 = S[0].m[2][0];
        //  Float s021 = S[0].m[2][1];
        //  Float s022 = S[0].m[2][2];
        //  Float s100 = S[1].m[0][0];
        //  Float s101 = S[1].m[0][1];
        //  Float s102 = S[1].m[0][2];
        //  Float s110 = S[1].m[1][0];
        //  Float s111 = S[1].m[1][1];
        //  Float s112 = S[1].m[1][2];
        //  Float s120 = S[1].m[2][0];
        //  Float s121 = S[1].m[2][1];
        //  Float s122 = S[1].m[2][2];

        //  c1[0] = DerivativeTerm(
        //      -t0x + t1x,
        //      (-1 + q1y * q1y + q1z * q1z + qperpy * qperpy + qperpz * qperpz) *
        //              s000 +
        //          q1w * q1z * s010 - qperpx * qperpy * s010 +
        //          qperpw * qperpz * s010 - q1w * q1y * s020 -
        //          qperpw * qperpy * s020 - qperpx * qperpz * s020 + s100 -
        //          q1y * q1y * s100 - q1z * q1z * s100 - qperpy * qperpy * s100 -
        //          qperpz * qperpz * s100 - q1w * q1z * s110 +
        //          qperpx * qperpy * s110 - qperpw * qperpz * s110 +
        //          q1w * q1y * s120 + qperpw * qperpy * s120 +
        //          qperpx * qperpz * s120 +
        //          q1x * (-(q1y * s010) - q1z * s020 + q1y * s110 + q1z * s120),
        //      (-1 + q1y * q1y + q1z * q1z + qperpy * qperpy + qperpz * qperpz) *
        //              s001 +
        //          q1w * q1z * s011 - qperpx * qperpy * s011 +
        //          qperpw * qperpz * s011 - q1w * q1y * s021 -
        //          qperpw * qperpy * s021 - qperpx * qperpz * s021 + s101 -
        //          q1y * q1y * s101 - q1z * q1z * s101 - qperpy * qperpy * s101 -
        //          qperpz * qperpz * s101 - q1w * q1z * s111 +
        //          qperpx * qperpy * s111 - qperpw * qperpz * s111 +
        //          q1w * q1y * s121 + qperpw * qperpy * s121 +
        //          qperpx * qperpz * s121 +
        //          q1x * (-(q1y * s011) - q1z * s021 + q1y * s111 + q1z * s121),
        //      (-1 + q1y * q1y + q1z * q1z + qperpy * qperpy + qperpz * qperpz) *
        //              s002 +
        //          q1w * q1z * s012 - qperpx * qperpy * s012 +
        //          qperpw * qperpz * s012 - q1w * q1y * s022 -
        //          qperpw * qperpy * s022 - qperpx * qperpz * s022 + s102 -
        //          q1y * q1y * s102 - q1z * q1z * s102 - qperpy * qperpy * s102 -
        //          qperpz * qperpz * s102 - q1w * q1z * s112 +
        //          qperpx * qperpy * s112 - qperpw * qperpz * s112 +
        //          q1w * q1y * s122 + qperpw * qperpy * s122 +
        //          qperpx * qperpz * s122 +
        //          q1x * (-(q1y * s012) - q1z * s022 + q1y * s112 + q1z * s122));

        //  c2[0] = DerivativeTerm(
        //      0.,
        //      -(qperpy * qperpy * s000) - qperpz * qperpz * s000 +
        //          qperpx * qperpy * s010 - qperpw * qperpz * s010 +
        //          qperpw * qperpy * s020 + qperpx * qperpz * s020 +
        //          q1y * q1y * (s000 - s100) + q1z * q1z * (s000 - s100) +
        //          qperpy * qperpy * s100 + qperpz * qperpz * s100 -
        //          qperpx * qperpy * s110 + qperpw * qperpz * s110 -
        //          qperpw * qperpy * s120 - qperpx * qperpz * s120 +
        //          2 * q1x * qperpy * s010 * theta -
        //          2 * q1w * qperpz * s010 * theta +
        //          2 * q1w * qperpy * s020 * theta +
        //          2 * q1x * qperpz * s020 * theta +
        //          q1y *
        //              (q1x * (-s010 + s110) + q1w * (-s020 + s120) +
        //               2 * (-2 * qperpy * s000 + qperpx * s010 + qperpw * s020) *
        //                   theta) +
        //          q1z * (q1w * (s010 - s110) + q1x * (-s020 + s120) -
        //                 2 * (2 * qperpz * s000 + qperpw * s010 - qperpx * s020) *
        //                     theta),
        //      -(qperpy * qperpy * s001) - qperpz * qperpz * s001 +
        //          qperpx * qperpy * s011 - qperpw * qperpz * s011 +
        //          qperpw * qperpy * s021 + qperpx * qperpz * s021 +
        //          q1y * q1y * (s001 - s101) + q1z * q1z * (s001 - s101) +
        //          qperpy * qperpy * s101 + qperpz * qperpz * s101 -
        //          qperpx * qperpy * s111 + qperpw * qperpz * s111 -
        //          qperpw * qperpy * s121 - qperpx * qperpz * s121 +
        //          2 * q1x * qperpy * s011 * theta -
        //          2 * q1w * qperpz * s011 * theta +
        //          2 * q1w * qperpy * s021 * theta +
        //          2 * q1x * qperpz * s021 * theta +
        //          q1y *
        //              (q1x * (-s011 + s111) + q1w * (-s021 + s121) +
        //               2 * (-2 * qperpy * s001 + qperpx * s011 + qperpw * s021) *
        //                   theta) +
        //          q1z * (q1w * (s011 - s111) + q1x * (-s021 + s121) -
        //                 2 * (2 * qperpz * s001 + qperpw * s011 - qperpx * s021) *
        //                     theta),
        //      -(qperpy * qperpy * s002) - qperpz * qperpz * s002 +
        //          qperpx * qperpy * s012 - qperpw * qperpz * s012 +
        //          qperpw * qperpy * s022 + qperpx * qperpz * s022 +
        //          q1y * q1y * (s002 - s102) + q1z * q1z * (s002 - s102) +
        //          qperpy * qperpy * s102 + qperpz * qperpz * s102 -
        //          qperpx * qperpy * s112 + qperpw * qperpz * s112 -
        //          qperpw * qperpy * s122 - qperpx * qperpz * s122 +
        //          2 * q1x * qperpy * s012 * theta -
        //          2 * q1w * qperpz * s012 * theta +
        //          2 * q1w * qperpy * s022 * theta +
        //          2 * q1x * qperpz * s022 * theta +
        //          q1y *
        //              (q1x * (-s012 + s112) + q1w * (-s022 + s122) +
        //               2 * (-2 * qperpy * s002 + qperpx * s012 + qperpw * s022) *
        //                   theta) +
        //          q1z * (q1w * (s012 - s112) + q1x * (-s022 + s122) -
        //                 2 * (2 * qperpz * s002 + qperpw * s012 - qperpx * s022) *
        //                     theta));

        //  c3[0] = DerivativeTerm(
        //      0.,
        //      -2 * (q1x * qperpy * s010 - q1w * qperpz * s010 +
        //            q1w * qperpy * s020 + q1x * qperpz * s020 -
        //            q1x * qperpy * s110 + q1w * qperpz * s110 -
        //            q1w * qperpy * s120 - q1x * qperpz * s120 +
        //            q1y * (-2 * qperpy * s000 + qperpx * s010 + qperpw * s020 +
        //                   2 * qperpy * s100 - qperpx * s110 - qperpw * s120) +
        //            q1z * (-2 * qperpz * s000 - qperpw * s010 + qperpx * s020 +
        //                   2 * qperpz * s100 + qperpw * s110 - qperpx * s120)) *
        //          theta,
        //      -2 * (q1x * qperpy * s011 - q1w * qperpz * s011 +
        //            q1w * qperpy * s021 + q1x * qperpz * s021 -
        //            q1x * qperpy * s111 + q1w * qperpz * s111 -
        //            q1w * qperpy * s121 - q1x * qperpz * s121 +
        //            q1y * (-2 * qperpy * s001 + qperpx * s011 + qperpw * s021 +
        //                   2 * qperpy * s101 - qperpx * s111 - qperpw * s121) +
        //            q1z * (-2 * qperpz * s001 - qperpw * s011 + qperpx * s021 +
        //                   2 * qperpz * s101 + qperpw * s111 - qperpx * s121)) *
        //          theta,
        //      -2 * (q1x * qperpy * s012 - q1w * qperpz * s012 +
        //            q1w * qperpy * s022 + q1x * qperpz * s022 -
        //            q1x * qperpy * s112 + q1w * qperpz * s112 -
        //            q1w * qperpy * s122 - q1x * qperpz * s122 +
        //            q1y * (-2 * qperpy * s002 + qperpx * s012 + qperpw * s022 +
        //                   2 * qperpy * s102 - qperpx * s112 - qperpw * s122) +
        //            q1z * (-2 * qperpz * s002 - qperpw * s012 + qperpx * s022 +
        //                   2 * qperpz * s102 + qperpw * s112 - qperpx * s122)) *
        //          theta);

        //  c4[0] = DerivativeTerm(
        //      0.,
        //      -(q1x * qperpy * s010) + q1w * qperpz * s010 - q1w * qperpy * s020 -
        //          q1x * qperpz * s020 + q1x * qperpy * s110 -
        //          q1w * qperpz * s110 + q1w * qperpy * s120 +
        //          q1x * qperpz * s120 + 2 * q1y * q1y * s000 * theta +
        //          2 * q1z * q1z * s000 * theta -
        //          2 * qperpy * qperpy * s000 * theta -
        //          2 * qperpz * qperpz * s000 * theta +
        //          2 * qperpx * qperpy * s010 * theta -
        //          2 * qperpw * qperpz * s010 * theta +
        //          2 * qperpw * qperpy * s020 * theta +
        //          2 * qperpx * qperpz * s020 * theta +
        //          q1y * (-(qperpx * s010) - qperpw * s020 +
        //                 2 * qperpy * (s000 - s100) + qperpx * s110 +
        //                 qperpw * s120 - 2 * q1x * s010 * theta -
        //                 2 * q1w * s020 * theta) +
        //          q1z * (2 * qperpz * s000 + qperpw * s010 - qperpx * s020 -
        //                 2 * qperpz * s100 - qperpw * s110 + qperpx * s120 +
        //                 2 * q1w * s010 * theta - 2 * q1x * s020 * theta),
        //      -(q1x * qperpy * s011) + q1w * qperpz * s011 - q1w * qperpy * s021 -
        //          q1x * qperpz * s021 + q1x * qperpy * s111 -
        //          q1w * qperpz * s111 + q1w * qperpy * s121 +
        //          q1x * qperpz * s121 + 2 * q1y * q1y * s001 * theta +
        //          2 * q1z * q1z * s001 * theta -
        //          2 * qperpy * qperpy * s001 * theta -
        //          2 * qperpz * qperpz * s001 * theta +
        //          2 * qperpx * qperpy * s011 * theta -
        //          2 * qperpw * qperpz * s011 * theta +
        //          2 * qperpw * qperpy * s021 * theta +
        //          2 * qperpx * qperpz * s021 * theta +
        //          q1y * (-(qperpx * s011) - qperpw * s021 +
        //                 2 * qperpy * (s001 - s101) + qperpx * s111 +
        //                 qperpw * s121 - 2 * q1x * s011 * theta -
        //                 2 * q1w * s021 * theta) +
        //          q1z * (2 * qperpz * s001 + qperpw * s011 - qperpx * s021 -
        //                 2 * qperpz * s101 - qperpw * s111 + qperpx * s121 +
        //                 2 * q1w * s011 * theta - 2 * q1x * s021 * theta),
        //      -(q1x * qperpy * s012) + q1w * qperpz * s012 - q1w * qperpy * s022 -
        //          q1x * qperpz * s022 + q1x * qperpy * s112 -
        //          q1w * qperpz * s112 + q1w * qperpy * s122 +
        //          q1x * qperpz * s122 + 2 * q1y * q1y * s002 * theta +
        //          2 * q1z * q1z * s002 * theta -
        //          2 * qperpy * qperpy * s002 * theta -
        //          2 * qperpz * qperpz * s002 * theta +
        //          2 * qperpx * qperpy * s012 * theta -
        //          2 * qperpw * qperpz * s012 * theta +
        //          2 * qperpw * qperpy * s022 * theta +
        //          2 * qperpx * qperpz * s022 * theta +
        //          q1y * (-(qperpx * s012) - qperpw * s022 +
        //                 2 * qperpy * (s002 - s102) + qperpx * s112 +
        //                 qperpw * s122 - 2 * q1x * s012 * theta -
        //                 2 * q1w * s022 * theta) +
        //          q1z * (2 * qperpz * s002 + qperpw * s012 - qperpx * s022 -
        //                 2 * qperpz * s102 - qperpw * s112 + qperpx * s122 +
        //                 2 * q1w * s012 * theta - 2 * q1x * s022 * theta));

        //  c5[0] = DerivativeTerm(
        //      0.,
        //      2 * (qperpy * qperpy * s000 + qperpz * qperpz * s000 -
        //           qperpx * qperpy * s010 + qperpw * qperpz * s010 -
        //           qperpw * qperpy * s020 - qperpx * qperpz * s020 -
        //           qperpy * qperpy * s100 - qperpz * qperpz * s100 +
        //           q1y * q1y * (-s000 + s100) + q1z * q1z * (-s000 + s100) +
        //           qperpx * qperpy * s110 - qperpw * qperpz * s110 +
        //           q1y * (q1x * (s010 - s110) + q1w * (s020 - s120)) +
        //           qperpw * qperpy * s120 + qperpx * qperpz * s120 +
        //           q1z * (-(q1w * s010) + q1x * s020 + q1w * s110 - q1x * s120)) *
        //          theta,
        //      2 * (qperpy * qperpy * s001 + qperpz * qperpz * s001 -
        //           qperpx * qperpy * s011 + qperpw * qperpz * s011 -
        //           qperpw * qperpy * s021 - qperpx * qperpz * s021 -
        //           qperpy * qperpy * s101 - qperpz * qperpz * s101 +
        //           q1y * q1y * (-s001 + s101) + q1z * q1z * (-s001 + s101) +
        //           qperpx * qperpy * s111 - qperpw * qperpz * s111 +
        //           q1y * (q1x * (s011 - s111) + q1w * (s021 - s121)) +
        //           qperpw * qperpy * s121 + qperpx * qperpz * s121 +
        //           q1z * (-(q1w * s011) + q1x * s021 + q1w * s111 - q1x * s121)) *
        //          theta,
        //      2 * (qperpy * qperpy * s002 + qperpz * qperpz * s002 -
        //           qperpx * qperpy * s012 + qperpw * qperpz * s012 -
        //           qperpw * qperpy * s022 - qperpx * qperpz * s022 -
        //           qperpy * qperpy * s102 - qperpz * qperpz * s102 +
        //           q1y * q1y * (-s002 + s102) + q1z * q1z * (-s002 + s102) +
        //           qperpx * qperpy * s112 - qperpw * qperpz * s112 +
        //           q1y * (q1x * (s012 - s112) + q1w * (s022 - s122)) +
        //           qperpw * qperpy * s122 + qperpx * qperpz * s122 +
        //           q1z * (-(q1w * s012) + q1x * s022 + q1w * s112 - q1x * s122)) *
        //          theta);

        //  c1[1] = DerivativeTerm(
        //      -t0y + t1y,
        //      -(qperpx * qperpy * s000) - qperpw * qperpz * s000 - s010 +
        //          q1z * q1z * s010 + qperpx * qperpx * s010 +
        //          qperpz * qperpz * s010 - q1y * q1z * s020 +
        //          qperpw * qperpx * s020 - qperpy * qperpz * s020 +
        //          qperpx * qperpy * s100 + qperpw * qperpz * s100 +
        //          q1w * q1z * (-s000 + s100) + q1x * q1x * (s010 - s110) + s110 -
        //          q1z * q1z * s110 - qperpx * qperpx * s110 -
        //          qperpz * qperpz * s110 +
        //          q1x * (q1y * (-s000 + s100) + q1w * (s020 - s120)) +
        //          q1y * q1z * s120 - qperpw * qperpx * s120 +
        //          qperpy * qperpz * s120,
        //      -(qperpx * qperpy * s001) - qperpw * qperpz * s001 - s011 +
        //          q1z * q1z * s011 + qperpx * qperpx * s011 +
        //          qperpz * qperpz * s011 - q1y * q1z * s021 +
        //          qperpw * qperpx * s021 - qperpy * qperpz * s021 +
        //          qperpx * qperpy * s101 + qperpw * qperpz * s101 +
        //          q1w * q1z * (-s001 + s101) + q1x * q1x * (s011 - s111) + s111 -
        //          q1z * q1z * s111 - qperpx * qperpx * s111 -
        //          qperpz * qperpz * s111 +
        //          q1x * (q1y * (-s001 + s101) + q1w * (s021 - s121)) +
        //          q1y * q1z * s121 - qperpw * qperpx * s121 +
        //          qperpy * qperpz * s121,
        //      -(qperpx * qperpy * s002) - qperpw * qperpz * s002 - s012 +
        //          q1z * q1z * s012 + qperpx * qperpx * s012 +
        //          qperpz * qperpz * s012 - q1y * q1z * s022 +
        //          qperpw * qperpx * s022 - qperpy * qperpz * s022 +
        //          qperpx * qperpy * s102 + qperpw * qperpz * s102 +
        //          q1w * q1z * (-s002 + s102) + q1x * q1x * (s012 - s112) + s112 -
        //          q1z * q1z * s112 - qperpx * qperpx * s112 -
        //          qperpz * qperpz * s112 +
        //          q1x * (q1y * (-s002 + s102) + q1w * (s022 - s122)) +
        //          q1y * q1z * s122 - qperpw * qperpx * s122 +
        //          qperpy * qperpz * s122);

        //  c2[1] = DerivativeTerm(
        //      0.,
        //      qperpx * qperpy * s000 + qperpw * qperpz * s000 + q1z * q1z * s010 -
        //          qperpx * qperpx * s010 - qperpz * qperpz * s010 -
        //          q1y * q1z * s020 - qperpw * qperpx * s020 +
        //          qperpy * qperpz * s020 - qperpx * qperpy * s100 -
        //          qperpw * qperpz * s100 + q1x * q1x * (s010 - s110) -
        //          q1z * q1z * s110 + qperpx * qperpx * s110 +
        //          qperpz * qperpz * s110 + q1y * q1z * s120 +
        //          qperpw * qperpx * s120 - qperpy * qperpz * s120 +
        //          2 * q1z * qperpw * s000 * theta +
        //          2 * q1y * qperpx * s000 * theta -
        //          4 * q1z * qperpz * s010 * theta +
        //          2 * q1z * qperpy * s020 * theta +
        //          2 * q1y * qperpz * s020 * theta +
        //          q1x * (q1w * s020 + q1y * (-s000 + s100) - q1w * s120 +
        //                 2 * qperpy * s000 * theta - 4 * qperpx * s010 * theta -
        //                 2 * qperpw * s020 * theta) +
        //          q1w * (-(q1z * s000) + q1z * s100 + 2 * qperpz * s000 * theta -
        //                 2 * qperpx * s020 * theta),
        //      qperpx * qperpy * s001 + qperpw * qperpz * s001 + q1z * q1z * s011 -
        //          qperpx * qperpx * s011 - qperpz * qperpz * s011 -
        //          q1y * q1z * s021 - qperpw * qperpx * s021 +
        //          qperpy * qperpz * s021 - qperpx * qperpy * s101 -
        //          qperpw * qperpz * s101 + q1x * q1x * (s011 - s111) -
        //          q1z * q1z * s111 + qperpx * qperpx * s111 +
        //          qperpz * qperpz * s111 + q1y * q1z * s121 +
        //          qperpw * qperpx * s121 - qperpy * qperpz * s121 +
        //          2 * q1z * qperpw * s001 * theta +
        //          2 * q1y * qperpx * s001 * theta -
        //          4 * q1z * qperpz * s011 * theta +
        //          2 * q1z * qperpy * s021 * theta +
        //          2 * q1y * qperpz * s021 * theta +
        //          q1x * (q1w * s021 + q1y * (-s001 + s101) - q1w * s121 +
        //                 2 * qperpy * s001 * theta - 4 * qperpx * s011 * theta -
        //                 2 * qperpw * s021 * theta) +
        //          q1w * (-(q1z * s001) + q1z * s101 + 2 * qperpz * s001 * theta -
        //                 2 * qperpx * s021 * theta),
        //      qperpx * qperpy * s002 + qperpw * qperpz * s002 + q1z * q1z * s012 -
        //          qperpx * qperpx * s012 - qperpz * qperpz * s012 -
        //          q1y * q1z * s022 - qperpw * qperpx * s022 +
        //          qperpy * qperpz * s022 - qperpx * qperpy * s102 -
        //          qperpw * qperpz * s102 + q1x * q1x * (s012 - s112) -
        //          q1z * q1z * s112 + qperpx * qperpx * s112 +
        //          qperpz * qperpz * s112 + q1y * q1z * s122 +
        //          qperpw * qperpx * s122 - qperpy * qperpz * s122 +
        //          2 * q1z * qperpw * s002 * theta +
        //          2 * q1y * qperpx * s002 * theta -
        //          4 * q1z * qperpz * s012 * theta +
        //          2 * q1z * qperpy * s022 * theta +
        //          2 * q1y * qperpz * s022 * theta +
        //          q1x * (q1w * s022 + q1y * (-s002 + s102) - q1w * s122 +
        //                 2 * qperpy * s002 * theta - 4 * qperpx * s012 * theta -
        //                 2 * qperpw * s022 * theta) +
        //          q1w * (-(q1z * s002) + q1z * s102 + 2 * qperpz * s002 * theta -
        //                 2 * qperpx * s022 * theta));

        //  c3[1] = DerivativeTerm(
        //      0., 2 * (-(q1x * qperpy * s000) - q1w * qperpz * s000 +
        //               2 * q1x * qperpx * s010 + q1x * qperpw * s020 +
        //               q1w * qperpx * s020 + q1x * qperpy * s100 +
        //               q1w * qperpz * s100 - 2 * q1x * qperpx * s110 -
        //               q1x * qperpw * s120 - q1w * qperpx * s120 +
        //               q1z * (2 * qperpz * s010 - qperpy * s020 +
        //                      qperpw * (-s000 + s100) - 2 * qperpz * s110 +
        //                      qperpy * s120) +
        //               q1y * (-(qperpx * s000) - qperpz * s020 + qperpx * s100 +
        //                      qperpz * s120)) *
        //              theta,
        //      2 * (-(q1x * qperpy * s001) - q1w * qperpz * s001 +
        //           2 * q1x * qperpx * s011 + q1x * qperpw * s021 +
        //           q1w * qperpx * s021 + q1x * qperpy * s101 +
        //           q1w * qperpz * s101 - 2 * q1x * qperpx * s111 -
        //           q1x * qperpw * s121 - q1w * qperpx * s121 +
        //           q1z * (2 * qperpz * s011 - qperpy * s021 +
        //                  qperpw * (-s001 + s101) - 2 * qperpz * s111 +
        //                  qperpy * s121) +
        //           q1y * (-(qperpx * s001) - qperpz * s021 + qperpx * s101 +
        //                  qperpz * s121)) *
        //          theta,
        //      2 * (-(q1x * qperpy * s002) - q1w * qperpz * s002 +
        //           2 * q1x * qperpx * s012 + q1x * qperpw * s022 +
        //           q1w * qperpx * s022 + q1x * qperpy * s102 +
        //           q1w * qperpz * s102 - 2 * q1x * qperpx * s112 -
        //           q1x * qperpw * s122 - q1w * qperpx * s122 +
        //           q1z * (2 * qperpz * s012 - qperpy * s022 +
        //                  qperpw * (-s002 + s102) - 2 * qperpz * s112 +
        //                  qperpy * s122) +
        //           q1y * (-(qperpx * s002) - qperpz * s022 + qperpx * s102 +
        //                  qperpz * s122)) *
        //          theta);

        //  c4[1] = DerivativeTerm(
        //      0.,
        //      -(q1x * qperpy * s000) - q1w * qperpz * s000 +
        //          2 * q1x * qperpx * s010 + q1x * qperpw * s020 +
        //          q1w * qperpx * s020 + q1x * qperpy * s100 +
        //          q1w * qperpz * s100 - 2 * q1x * qperpx * s110 -
        //          q1x * qperpw * s120 - q1w * qperpx * s120 +
        //          2 * qperpx * qperpy * s000 * theta +
        //          2 * qperpw * qperpz * s000 * theta +
        //          2 * q1x * q1x * s010 * theta + 2 * q1z * q1z * s010 * theta -
        //          2 * qperpx * qperpx * s010 * theta -
        //          2 * qperpz * qperpz * s010 * theta +
        //          2 * q1w * q1x * s020 * theta -
        //          2 * qperpw * qperpx * s020 * theta +
        //          2 * qperpy * qperpz * s020 * theta +
        //          q1y * (-(qperpx * s000) - qperpz * s020 + qperpx * s100 +
        //                 qperpz * s120 - 2 * q1x * s000 * theta) +
        //          q1z * (2 * qperpz * s010 - qperpy * s020 +
        //                 qperpw * (-s000 + s100) - 2 * qperpz * s110 +
        //                 qperpy * s120 - 2 * q1w * s000 * theta -
        //                 2 * q1y * s020 * theta),
        //      -(q1x * qperpy * s001) - q1w * qperpz * s001 +
        //          2 * q1x * qperpx * s011 + q1x * qperpw * s021 +
        //          q1w * qperpx * s021 + q1x * qperpy * s101 +
        //          q1w * qperpz * s101 - 2 * q1x * qperpx * s111 -
        //          q1x * qperpw * s121 - q1w * qperpx * s121 +
        //          2 * qperpx * qperpy * s001 * theta +
        //          2 * qperpw * qperpz * s001 * theta +
        //          2 * q1x * q1x * s011 * theta + 2 * q1z * q1z * s011 * theta -
        //          2 * qperpx * qperpx * s011 * theta -
        //          2 * qperpz * qperpz * s011 * theta +
        //          2 * q1w * q1x * s021 * theta -
        //          2 * qperpw * qperpx * s021 * theta +
        //          2 * qperpy * qperpz * s021 * theta +
        //          q1y * (-(qperpx * s001) - qperpz * s021 + qperpx * s101 +
        //                 qperpz * s121 - 2 * q1x * s001 * theta) +
        //          q1z * (2 * qperpz * s011 - qperpy * s021 +
        //                 qperpw * (-s001 + s101) - 2 * qperpz * s111 +
        //                 qperpy * s121 - 2 * q1w * s001 * theta -
        //                 2 * q1y * s021 * theta),
        //      -(q1x * qperpy * s002) - q1w * qperpz * s002 +
        //          2 * q1x * qperpx * s012 + q1x * qperpw * s022 +
        //          q1w * qperpx * s022 + q1x * qperpy * s102 +
        //          q1w * qperpz * s102 - 2 * q1x * qperpx * s112 -
        //          q1x * qperpw * s122 - q1w * qperpx * s122 +
        //          2 * qperpx * qperpy * s002 * theta +
        //          2 * qperpw * qperpz * s002 * theta +
        //          2 * q1x * q1x * s012 * theta + 2 * q1z * q1z * s012 * theta -
        //          2 * qperpx * qperpx * s012 * theta -
        //          2 * qperpz * qperpz * s012 * theta +
        //          2 * q1w * q1x * s022 * theta -
        //          2 * qperpw * qperpx * s022 * theta +
        //          2 * qperpy * qperpz * s022 * theta +
        //          q1y * (-(qperpx * s002) - qperpz * s022 + qperpx * s102 +
        //                 qperpz * s122 - 2 * q1x * s002 * theta) +
        //          q1z * (2 * qperpz * s012 - qperpy * s022 +
        //                 qperpw * (-s002 + s102) - 2 * qperpz * s112 +
        //                 qperpy * s122 - 2 * q1w * s002 * theta -
        //                 2 * q1y * s022 * theta));

        //  c5[1] = DerivativeTerm(
        //      0., -2 * (qperpx * qperpy * s000 + qperpw * qperpz * s000 +
        //                q1z * q1z * s010 - qperpx * qperpx * s010 -
        //                qperpz * qperpz * s010 - q1y * q1z * s020 -
        //                qperpw * qperpx * s020 + qperpy * qperpz * s020 -
        //                qperpx * qperpy * s100 - qperpw * qperpz * s100 +
        //                q1w * q1z * (-s000 + s100) + q1x * q1x * (s010 - s110) -
        //                q1z * q1z * s110 + qperpx * qperpx * s110 +
        //                qperpz * qperpz * s110 +
        //                q1x * (q1y * (-s000 + s100) + q1w * (s020 - s120)) +
        //                q1y * q1z * s120 + qperpw * qperpx * s120 -
        //                qperpy * qperpz * s120) *
        //              theta,
        //      -2 * (qperpx * qperpy * s001 + qperpw * qperpz * s001 +
        //            q1z * q1z * s011 - qperpx * qperpx * s011 -
        //            qperpz * qperpz * s011 - q1y * q1z * s021 -
        //            qperpw * qperpx * s021 + qperpy * qperpz * s021 -
        //            qperpx * qperpy * s101 - qperpw * qperpz * s101 +
        //            q1w * q1z * (-s001 + s101) + q1x * q1x * (s011 - s111) -
        //            q1z * q1z * s111 + qperpx * qperpx * s111 +
        //            qperpz * qperpz * s111 +
        //            q1x * (q1y * (-s001 + s101) + q1w * (s021 - s121)) +
        //            q1y * q1z * s121 + qperpw * qperpx * s121 -
        //            qperpy * qperpz * s121) *
        //          theta,
        //      -2 * (qperpx * qperpy * s002 + qperpw * qperpz * s002 +
        //            q1z * q1z * s012 - qperpx * qperpx * s012 -
        //            qperpz * qperpz * s012 - q1y * q1z * s022 -
        //            qperpw * qperpx * s022 + qperpy * qperpz * s022 -
        //            qperpx * qperpy * s102 - qperpw * qperpz * s102 +
        //            q1w * q1z * (-s002 + s102) + q1x * q1x * (s012 - s112) -
        //            q1z * q1z * s112 + qperpx * qperpx * s112 +
        //            qperpz * qperpz * s112 +
        //            q1x * (q1y * (-s002 + s102) + q1w * (s022 - s122)) +
        //            q1y * q1z * s122 + qperpw * qperpx * s122 -
        //            qperpy * qperpz * s122) *
        //          theta);

        //  c1[2] = DerivativeTerm(
        //      -t0z + t1z, (qperpw * qperpy * s000 - qperpx * qperpz * s000 -
        //                   q1y * q1z * s010 - qperpw * qperpx * s010 -
        //                   qperpy * qperpz * s010 - s020 + q1y * q1y * s020 +
        //                   qperpx * qperpx * s020 + qperpy * qperpy * s020 -
        //                   qperpw * qperpy * s100 + qperpx * qperpz * s100 +
        //                   q1x * q1z * (-s000 + s100) + q1y * q1z * s110 +
        //                   qperpw * qperpx * s110 + qperpy * qperpz * s110 +
        //                   q1w * (q1y * (s000 - s100) + q1x * (-s010 + s110)) +
        //                   q1x * q1x * (s020 - s120) + s120 - q1y * q1y * s120 -
        //                   qperpx * qperpx * s120 - qperpy * qperpy * s120),
        //      (qperpw * qperpy * s001 - qperpx * qperpz * s001 -
        //       q1y * q1z * s011 - qperpw * qperpx * s011 -
        //       qperpy * qperpz * s011 - s021 + q1y * q1y * s021 +
        //       qperpx * qperpx * s021 + qperpy * qperpy * s021 -
        //       qperpw * qperpy * s101 + qperpx * qperpz * s101 +
        //       q1x * q1z * (-s001 + s101) + q1y * q1z * s111 +
        //       qperpw * qperpx * s111 + qperpy * qperpz * s111 +
        //       q1w * (q1y * (s001 - s101) + q1x * (-s011 + s111)) +
        //       q1x * q1x * (s021 - s121) + s121 - q1y * q1y * s121 -
        //       qperpx * qperpx * s121 - qperpy * qperpy * s121),
        //      (qperpw * qperpy * s002 - qperpx * qperpz * s002 -
        //       q1y * q1z * s012 - qperpw * qperpx * s012 -
        //       qperpy * qperpz * s012 - s022 + q1y * q1y * s022 +
        //       qperpx * qperpx * s022 + qperpy * qperpy * s022 -
        //       qperpw * qperpy * s102 + qperpx * qperpz * s102 +
        //       q1x * q1z * (-s002 + s102) + q1y * q1z * s112 +
        //       qperpw * qperpx * s112 + qperpy * qperpz * s112 +
        //       q1w * (q1y * (s002 - s102) + q1x * (-s012 + s112)) +
        //       q1x * q1x * (s022 - s122) + s122 - q1y * q1y * s122 -
        //       qperpx * qperpx * s122 - qperpy * qperpy * s122));

        //  c2[2] = DerivativeTerm(
        //      0.,
        //      (q1w * q1y * s000 - q1x * q1z * s000 - qperpw * qperpy * s000 +
        //       qperpx * qperpz * s000 - q1w * q1x * s010 - q1y * q1z * s010 +
        //       qperpw * qperpx * s010 + qperpy * qperpz * s010 +
        //       q1x * q1x * s020 + q1y * q1y * s020 - qperpx * qperpx * s020 -
        //       qperpy * qperpy * s020 - q1w * q1y * s100 + q1x * q1z * s100 +
        //       qperpw * qperpy * s100 - qperpx * qperpz * s100 +
        //       q1w * q1x * s110 + q1y * q1z * s110 - qperpw * qperpx * s110 -
        //       qperpy * qperpz * s110 - q1x * q1x * s120 - q1y * q1y * s120 +
        //       qperpx * qperpx * s120 + qperpy * qperpy * s120 -
        //       2 * q1y * qperpw * s000 * theta + 2 * q1z * qperpx * s000 * theta -
        //       2 * q1w * qperpy * s000 * theta + 2 * q1x * qperpz * s000 * theta +
        //       2 * q1x * qperpw * s010 * theta + 2 * q1w * qperpx * s010 * theta +
        //       2 * q1z * qperpy * s010 * theta + 2 * q1y * qperpz * s010 * theta -
        //       4 * q1x * qperpx * s020 * theta - 4 * q1y * qperpy * s020 * theta),
        //      (q1w * q1y * s001 - q1x * q1z * s001 - qperpw * qperpy * s001 +
        //       qperpx * qperpz * s001 - q1w * q1x * s011 - q1y * q1z * s011 +
        //       qperpw * qperpx * s011 + qperpy * qperpz * s011 +
        //       q1x * q1x * s021 + q1y * q1y * s021 - qperpx * qperpx * s021 -
        //       qperpy * qperpy * s021 - q1w * q1y * s101 + q1x * q1z * s101 +
        //       qperpw * qperpy * s101 - qperpx * qperpz * s101 +
        //       q1w * q1x * s111 + q1y * q1z * s111 - qperpw * qperpx * s111 -
        //       qperpy * qperpz * s111 - q1x * q1x * s121 - q1y * q1y * s121 +
        //       qperpx * qperpx * s121 + qperpy * qperpy * s121 -
        //       2 * q1y * qperpw * s001 * theta + 2 * q1z * qperpx * s001 * theta -
        //       2 * q1w * qperpy * s001 * theta + 2 * q1x * qperpz * s001 * theta +
        //       2 * q1x * qperpw * s011 * theta + 2 * q1w * qperpx * s011 * theta +
        //       2 * q1z * qperpy * s011 * theta + 2 * q1y * qperpz * s011 * theta -
        //       4 * q1x * qperpx * s021 * theta - 4 * q1y * qperpy * s021 * theta),
        //      (q1w * q1y * s002 - q1x * q1z * s002 - qperpw * qperpy * s002 +
        //       qperpx * qperpz * s002 - q1w * q1x * s012 - q1y * q1z * s012 +
        //       qperpw * qperpx * s012 + qperpy * qperpz * s012 +
        //       q1x * q1x * s022 + q1y * q1y * s022 - qperpx * qperpx * s022 -
        //       qperpy * qperpy * s022 - q1w * q1y * s102 + q1x * q1z * s102 +
        //       qperpw * qperpy * s102 - qperpx * qperpz * s102 +
        //       q1w * q1x * s112 + q1y * q1z * s112 - qperpw * qperpx * s112 -
        //       qperpy * qperpz * s112 - q1x * q1x * s122 - q1y * q1y * s122 +
        //       qperpx * qperpx * s122 + qperpy * qperpy * s122 -
        //       2 * q1y * qperpw * s002 * theta + 2 * q1z * qperpx * s002 * theta -
        //       2 * q1w * qperpy * s002 * theta + 2 * q1x * qperpz * s002 * theta +
        //       2 * q1x * qperpw * s012 * theta + 2 * q1w * qperpx * s012 * theta +
        //       2 * q1z * qperpy * s012 * theta + 2 * q1y * qperpz * s012 * theta -
        //       4 * q1x * qperpx * s022 * theta -
        //       4 * q1y * qperpy * s022 * theta));

        //  c3[2] = DerivativeTerm(
        //      0., -2 * (-(q1w * qperpy * s000) + q1x * qperpz * s000 +
        //                q1x * qperpw * s010 + q1w * qperpx * s010 -
        //                2 * q1x * qperpx * s020 + q1w * qperpy * s100 -
        //                q1x * qperpz * s100 - q1x * qperpw * s110 -
        //                q1w * qperpx * s110 +
        //                q1z * (qperpx * s000 + qperpy * s010 - qperpx * s100 -
        //                       qperpy * s110) +
        //                2 * q1x * qperpx * s120 +
        //                q1y * (qperpz * s010 - 2 * qperpy * s020 +
        //                       qperpw * (-s000 + s100) - qperpz * s110 +
        //                       2 * qperpy * s120)) *
        //              theta,
        //      -2 * (-(q1w * qperpy * s001) + q1x * qperpz * s001 +
        //            q1x * qperpw * s011 + q1w * qperpx * s011 -
        //            2 * q1x * qperpx * s021 + q1w * qperpy * s101 -
        //            q1x * qperpz * s101 - q1x * qperpw * s111 -
        //            q1w * qperpx * s111 +
        //            q1z * (qperpx * s001 + qperpy * s011 - qperpx * s101 -
        //                   qperpy * s111) +
        //            2 * q1x * qperpx * s121 +
        //            q1y * (qperpz * s011 - 2 * qperpy * s021 +
        //                   qperpw * (-s001 + s101) - qperpz * s111 +
        //                   2 * qperpy * s121)) *
        //          theta,
        //      -2 * (-(q1w * qperpy * s002) + q1x * qperpz * s002 +
        //            q1x * qperpw * s012 + q1w * qperpx * s012 -
        //            2 * q1x * qperpx * s022 + q1w * qperpy * s102 -
        //            q1x * qperpz * s102 - q1x * qperpw * s112 -
        //            q1w * qperpx * s112 +
        //            q1z * (qperpx * s002 + qperpy * s012 - qperpx * s102 -
        //                   qperpy * s112) +
        //            2 * q1x * qperpx * s122 +
        //            q1y * (qperpz * s012 - 2 * qperpy * s022 +
        //                   qperpw * (-s002 + s102) - qperpz * s112 +
        //                   2 * qperpy * s122)) *
        //          theta);

        //  c4[2] = DerivativeTerm(
        //      0.,
        //      q1w * qperpy * s000 - q1x * qperpz * s000 - q1x * qperpw * s010 -
        //          q1w * qperpx * s010 + 2 * q1x * qperpx * s020 -
        //          q1w * qperpy * s100 + q1x * qperpz * s100 +
        //          q1x * qperpw * s110 + q1w * qperpx * s110 -
        //          2 * q1x * qperpx * s120 - 2 * qperpw * qperpy * s000 * theta +
        //          2 * qperpx * qperpz * s000 * theta -
        //          2 * q1w * q1x * s010 * theta +
        //          2 * qperpw * qperpx * s010 * theta +
        //          2 * qperpy * qperpz * s010 * theta +
        //          2 * q1x * q1x * s020 * theta + 2 * q1y * q1y * s020 * theta -
        //          2 * qperpx * qperpx * s020 * theta -
        //          2 * qperpy * qperpy * s020 * theta +
        //          q1z * (-(qperpx * s000) - qperpy * s010 + qperpx * s100 +
        //                 qperpy * s110 - 2 * q1x * s000 * theta) +
        //          q1y * (-(qperpz * s010) + 2 * qperpy * s020 +
        //                 qperpw * (s000 - s100) + qperpz * s110 -
        //                 2 * qperpy * s120 + 2 * q1w * s000 * theta -
        //                 2 * q1z * s010 * theta),
        //      q1w * qperpy * s001 - q1x * qperpz * s001 - q1x * qperpw * s011 -
        //          q1w * qperpx * s011 + 2 * q1x * qperpx * s021 -
        //          q1w * qperpy * s101 + q1x * qperpz * s101 +
        //          q1x * qperpw * s111 + q1w * qperpx * s111 -
        //          2 * q1x * qperpx * s121 - 2 * qperpw * qperpy * s001 * theta +
        //          2 * qperpx * qperpz * s001 * theta -
        //          2 * q1w * q1x * s011 * theta +
        //          2 * qperpw * qperpx * s011 * theta +
        //          2 * qperpy * qperpz * s011 * theta +
        //          2 * q1x * q1x * s021 * theta + 2 * q1y * q1y * s021 * theta -
        //          2 * qperpx * qperpx * s021 * theta -
        //          2 * qperpy * qperpy * s021 * theta +
        //          q1z * (-(qperpx * s001) - qperpy * s011 + qperpx * s101 +
        //                 qperpy * s111 - 2 * q1x * s001 * theta) +
        //          q1y * (-(qperpz * s011) + 2 * qperpy * s021 +
        //                 qperpw * (s001 - s101) + qperpz * s111 -
        //                 2 * qperpy * s121 + 2 * q1w * s001 * theta -
        //                 2 * q1z * s011 * theta),
        //      q1w * qperpy * s002 - q1x * qperpz * s002 - q1x * qperpw * s012 -
        //          q1w * qperpx * s012 + 2 * q1x * qperpx * s022 -
        //          q1w * qperpy * s102 + q1x * qperpz * s102 +
        //          q1x * qperpw * s112 + q1w * qperpx * s112 -
        //          2 * q1x * qperpx * s122 - 2 * qperpw * qperpy * s002 * theta +
        //          2 * qperpx * qperpz * s002 * theta -
        //          2 * q1w * q1x * s012 * theta +
        //          2 * qperpw * qperpx * s012 * theta +
        //          2 * qperpy * qperpz * s012 * theta +
        //          2 * q1x * q1x * s022 * theta + 2 * q1y * q1y * s022 * theta -
        //          2 * qperpx * qperpx * s022 * theta -
        //          2 * qperpy * qperpy * s022 * theta +
        //          q1z * (-(qperpx * s002) - qperpy * s012 + qperpx * s102 +
        //                 qperpy * s112 - 2 * q1x * s002 * theta) +
        //          q1y * (-(qperpz * s012) + 2 * qperpy * s022 +
        //                 qperpw * (s002 - s102) + qperpz * s112 -
        //                 2 * qperpy * s122 + 2 * q1w * s002 * theta -
        //                 2 * q1z * s012 * theta));

        //  c5[2] = DerivativeTerm(
        //      0., 2 * (qperpw * qperpy * s000 - qperpx * qperpz * s000 +
        //               q1y * q1z * s010 - qperpw * qperpx * s010 -
        //               qperpy * qperpz * s010 - q1y * q1y * s020 +
        //               qperpx * qperpx * s020 + qperpy * qperpy * s020 +
        //               q1x * q1z * (s000 - s100) - qperpw * qperpy * s100 +
        //               qperpx * qperpz * s100 +
        //               q1w * (q1y * (-s000 + s100) + q1x * (s010 - s110)) -
        //               q1y * q1z * s110 + qperpw * qperpx * s110 +
        //               qperpy * qperpz * s110 + q1y * q1y * s120 -
        //               qperpx * qperpx * s120 - qperpy * qperpy * s120 +
        //               q1x * q1x * (-s020 + s120)) *
        //              theta,
        //      2 * (qperpw * qperpy * s001 - qperpx * qperpz * s001 +
        //           q1y * q1z * s011 - qperpw * qperpx * s011 -
        //           qperpy * qperpz * s011 - q1y * q1y * s021 +
        //           qperpx * qperpx * s021 + qperpy * qperpy * s021 +
        //           q1x * q1z * (s001 - s101) - qperpw * qperpy * s101 +
        //           qperpx * qperpz * s101 +
        //           q1w * (q1y * (-s001 + s101) + q1x * (s011 - s111)) -
        //           q1y * q1z * s111 + qperpw * qperpx * s111 +
        //           qperpy * qperpz * s111 + q1y * q1y * s121 -
        //           qperpx * qperpx * s121 - qperpy * qperpy * s121 +
        //           q1x * q1x * (-s021 + s121)) *
        //          theta,
        //      2 * (qperpw * qperpy * s002 - qperpx * qperpz * s002 +
        //           q1y * q1z * s012 - qperpw * qperpx * s012 -
        //           qperpy * qperpz * s012 - q1y * q1y * s022 +
        //           qperpx * qperpx * s022 + qperpy * qperpy * s022 +
        //           q1x * q1z * (s002 - s102) - qperpw * qperpy * s102 +
        //           qperpx * qperpz * s102 +
        //           q1w * (q1y * (-s002 + s102) + q1x * (s012 - s112)) -
        //           q1y * q1z * s112 + qperpw * qperpx * s112 +
        //           qperpy * qperpz * s112 + q1y * q1y * s122 -
        //           qperpx * qperpx * s122 - qperpy * qperpy * s122 +
        //           q1x * q1x * (-s022 + s122)) *
        //          theta);
        //}
      }

    public static void Decompose(Matrix4x4 m, out Vector3D T, out Quaternion rQuat, out Matrix4x4 S)
    {
      throw new NotImplementedException();
    }

    public void Interpolate(double time, out Transform t)
    {
      // Handle boundary conditions for matrix interpolation
      if (!_actuallyAnimated || time <= _startTime)
      {
        t = _startTransform;
        return;
      }

      if (time >= _endTime)
      {
        t = _endTransform;
        return;
      }
      double dt = (time - _startTime) / (_endTime - _startTime);
      // Interpolate translation at _dt_
      Vector3D trans = (1.0 - dt) * T[0] + dt * T[1];

      // Interpolate rotation at _dt_
      Quaternion rotate = PbrtMath.Slerp(dt, R[0], R[1]);

      // Interpolate scale at _dt_
      double[,] mat = new double[4, 4];
      for (int i = 0; i < 3; ++i)
      {
        for (int j = 0; j < 3; ++j)
        {
          mat[i,j] = PbrtMath.Lerp(dt, S[0][i,j], S[1][i,j]);
        }
      }

      var scale = new Matrix4x4(mat);

      // Compute interpolated matrix as product of interpolated components
      t = Transform.Translate(trans) * rotate.ToTransform() * new Transform(scale);

    }

    // Ray operator()(const Ray &r) const;
    // RayDifferential operator()(const RayDifferential &r) const;
    // Point3D operator()(Float time, const Point3f &p) const;
    // Vector3f operator()(Float time, const Vector3f &v) const;
    public bool HasScale()
    {
      return _startTransform.HasScale() || _endTransform.HasScale();
    }

    public Bounds3D MotionBounds(Bounds3D b)
    {
      if (!_actuallyAnimated)
      {
        return _startTransform.AtBounds(b);
      }

      if (_hasRotation == false)
      {
        return _startTransform.AtBounds(b).Union(_endTransform.AtBounds(b));
      }

      // Return motion bounds accounting for animated rotation
      Bounds3D bounds = new Bounds3D();
      for (int corner = 0; corner < 8; ++corner)
      {
        bounds = bounds.Union(BoundPointMotion(b.Corner(corner)));
      }

      return bounds;
    }

    public Bounds3D BoundPointMotion(Point3D p)
    {
      if (!_actuallyAnimated)
      {
        return new Bounds3D(_startTransform.AtPoint(p));
      }

      throw new NotImplementedException();

      //Bounds3D bounds = new Bounds3D(_startTransform.AtPoint(p), _endTransform.AtPoint(p));
      //double cosTheta = R[0].Dot(R[1]);
      //double theta = Math.Acos(Clamp(cosTheta, -1, 1));
      //for (int c = 0; c < 3; ++c)
      //{
      //  // Find any motion derivative zeros for the component _c_
      //  double[] zeros = new double[8];
      //  int nZeros = 0;
      //  IntervalFindZeros(c1[c].Eval(p), c2[c].Eval(p), c3[c].Eval(p),
      //                    c4[c].Eval(p), c5[c].Eval(p), theta, Interval(0., 1.),
      //                    zeros, &nZeros);
      //  CHECK_LE(nZeros, sizeof(zeros) / sizeof(zeros[0]));

      //  // Expand bounding box for any motion derivative zeros found
      //  for (int i = 0; i < nZeros; ++i)
      //  {
      //    Point3f pz = (*this)(Lerp(zeros[i], startTime, endTime), p);
      //    bounds = Union(bounds, pz);
      //  }
      //}
      //return bounds;
    }

    public Ray ExecuteTransform(Ray ray)
    {
      if (!_actuallyAnimated || ray.Time <= _startTime)
      {
        return _startTransform.ExecuteTransform(ray);
      }
      else if (ray.Time >= _endTime)
      {
        return _endTransform.ExecuteTransform(ray);
      }
      else
      {
        Transform t;
        Interpolate(ray.Time, out t);
        return t.ExecuteTransform(ray);
      }
    }

    public Point3D ExecuteTransform(double time, Point3D p)
    {
      if (!_actuallyAnimated || time <= _startTime)
      {
        return _startTransform.ExecuteTransform(p);
      }
      else if (time >= _endTime)
      {
        return _endTransform.ExecuteTransform(p);
      }

      Interpolate(time, out Transform t);
      return t.ExecuteTransform(p);

    }

    //struct DerivativeTerm
    //{
    //  DerivativeTerm() { }
    //  DerivativeTerm(Float c, Float x, Float y, Float z)
    //    : kc(c), kx(x), ky(y), kz(z) { }
    //  Float kc, kx, ky, kz;
    //  Float Eval(const Point3f &p) const {
    //    return kc + kx* p.x + ky* p.y + kz* p.z;
    //  }
    //};
    //DerivativeTerm c1[3], c2[3], c3[3], c4[3], c5[3];
  }
}