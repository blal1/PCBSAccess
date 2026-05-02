using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Reviews app when it opens and when the rating changes.
    /// Patches ReviewApp.Awake (open) and UpdateReviews (rating refresh).
    /// </summary>
    public static class ReviewAppHandler
    {
        private static float _lastRating = -1f;

        public static void Reset()
        {
            _lastRating = -1f;
        }

        [HarmonyPatch(typeof(ReviewApp), "Awake")]
        static class ReviewApp_Awake_Patch
        {
            static void Postfix(ReviewApp __instance)
            {
                try
                {
                    string company = __instance.m_companyName.text;
                    float  rating  = CareerStatus.Get().GetStarRating();
                    _lastRating    = rating;

                    List<IReview> reviews = CareerStatus.Get().GetReviews();
                    int count = reviews != null ? reviews.Count : 0;

                    // Build summary: company + stars + top reviews
                    string summary = Loc.Get("review_open", company, $"{rating:F1}", count);
                    ScreenReader.Say(summary);

                    // Queue up to 3 most recent reviews
                    if (reviews != null)
                    {
                        int show = Math.Min(3, reviews.Count);
                        for (int i = reviews.Count - 1; i >= reviews.Count - show; i--)
                        {
                            string from   = reviews[i].GetFrom();
                            string body   = reviews[i].GetReview();
                            float  stars  = reviews[i].GetStars();
                            ScreenReader.Say($"{from}. {stars:F0} {Loc.Get("stars")}. {body}", interrupt: false);
                        }
                    }

                    DebugLogger.LogState($"ReviewAppHandler: opened, company='{company}', rating={rating:F1}, reviews={count}");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ReviewApp_Awake_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(ReviewApp), "UpdateReviews")]
        static class ReviewApp_UpdateReviews_Patch
        {
            static void Postfix()
            {
                try
                {
                    float rating = CareerStatus.Get().GetStarRating();
                    if (Math.Abs(rating - _lastRating) < 0.05f) return;
                    _lastRating = rating;
                    ScreenReader.Say(Loc.Get("review_updated", $"{rating:F1}"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"ReviewApp_UpdateReviews_Patch: {ex.Message}");
                }
            }
        }
    }
}
