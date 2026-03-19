using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ZPF.Graphics;

namespace ZPF.Skia;

public class ImageDiffResult
{
   /// <summary>
   /// Number of pixels above threshold
   /// </summary>
   public int ChangedPixels { get; set; }

   /// <summary>
   /// Percentage of total pixels changed
   /// </summary>
   public double Percentage { get; set; }

   /// <summary>
   /// List of bounding boxes where differences were found
   /// </summary>
   public List<SKRect> BoundingBoxes { get; set; } = new();
}

public static class ImageDiff
{
   /// <summary>
   /// Compares two images and generates a diff image highlighting the differences.
   /// </summary>
   /// <param name="img1Path"></param>
   /// <param name="img2Path"></param>
   /// <param name="diffOnlyPath">Red pixels = changed areas /  Green rectangle = bounding box</param>
   /// <param name="threshold">Threshold for pixel difference (0-765, where 765 means all RGB channels are different)</param>
   /// <param name="minRegionSize"></param>
   /// <param name="mergeDistance"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   public static ImageDiffResult CompareImages(
       string img1Path,
       string img2Path,
       string img2wDiffPath = null,
       string diffOnlyPath = null,
       int threshold = 10,
       int minRegionSize = 20,
       float mergeDistance = 10f)
   {
      using var bmp1 = SKBitmap.Decode(img1Path);
      using var bmp2 = SKBitmap.Decode(img2Path);

      if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
         throw new Exception("Images must have the same dimensions.");

      int width = bmp1.Width;
      int height = bmp1.Height;

      var diff = new SKBitmap(width, height);
      var diffMask = new bool[width, height];
      int changedPixels = 0;

      // -----------------------------
      // BUILD DIFF MASK
      // -----------------------------
      for (int y = 0; y < height; y++)
      {
         for (int x = 0; x < width; x++)
         {
            var c1 = bmp1.GetPixel(x, y);
            var c2 = bmp2.GetPixel(x, y);

            int dr = Math.Abs(c1.Red - c2.Red);
            int dg = Math.Abs(c1.Green - c2.Green);
            int db = Math.Abs(c1.Blue - c2.Blue);

            int diffValue = dr + dg + db;

            if (diffValue > threshold)
            {
               changedPixels++;
               diffMask[x, y] = true;
               diff.SetPixel(x, y, new SKColor(255, 0, 0));
            }
            else
            {
               diff.SetPixel(x, y, new SKColor(0, 0, 0, 0));
            }
         }
      }

      // -----------------------------
      // CONNECTED COMPONENTS (REGIONS)
      // -----------------------------
      var visited = new bool[width, height];
      var boundingBoxes = new List<SKRect>();
      var regionPixelCounts = new List<int>();

      int[][] dirs = new[]
      {
            new[] { 1, 0 }, new[] { -1, 0 },
            new[] { 0, 1 }, new[] { 0, -1 }
        };

      for (int y = 0; y < height; y++)
      {
         for (int x = 0; x < width; x++)
         {
            if (!diffMask[x, y] || visited[x, y])
               continue;

            int minX = x, minY = y, maxX = x, maxY = y;
            int pixelCount = 0;

            var stack = new Stack<(int X, int Y)>();
            stack.Push((x, y));
            visited[x, y] = true;

            while (stack.Count > 0)
            {
               var (cx, cy) = stack.Pop();
               pixelCount++;

               if (cx < minX) minX = cx;
               if (cy < minY) minY = cy;
               if (cx > maxX) maxX = cx;
               if (cy > maxY) maxY = cy;

               foreach (var d in dirs)
               {
                  int nx = cx + d[0];
                  int ny = cy + d[1];

                  if (nx >= 0 && nx < width &&
                      ny >= 0 && ny < height &&
                      diffMask[nx, ny] &&
                      !visited[nx, ny])
                  {
                     visited[nx, ny] = true;
                     stack.Push((nx, ny));
                  }
               }
            }

            boundingBoxes.Add(
               new SKRect(
                  Math.Max(0, minX - 2), Math.Max(0, minY - 2),
                  maxX + 4, maxY + 4));
            regionPixelCounts.Add(pixelCount);
         }
      }

      // -----------------------------
      // FILTER SMALL REGIONS
      // -----------------------------
      var filteredBoxes = new List<SKRect>();

      for (int i = 0; i < boundingBoxes.Count; i++)
      {
         if (regionPixelCounts[i] >= minRegionSize)
            filteredBoxes.Add(boundingBoxes[i]);
      }

      boundingBoxes = filteredBoxes;

      // -----------------------------
      // MERGE NEARBY BOUNDING BOXES
      // -----------------------------
      bool merged;
      do
      {
         merged = false;
         var newList = new List<SKRect>();

         for (int i = 0; i < boundingBoxes.Count; i++)
         {
            var a = boundingBoxes[i];
            bool hasMerged = false;

            for (int j = i + 1; j < boundingBoxes.Count; j++)
            {
               var b = boundingBoxes[j];

               var expandedA = new SKRect(
                   a.Left - mergeDistance,
                   a.Top - mergeDistance,
                   a.Right + mergeDistance,
                   a.Bottom + mergeDistance
               );

               if (expandedA.IntersectsWith(b))
               {
                  var mergedBox = new SKRect(
                      Math.Min(a.Left, b.Left),
                      Math.Min(a.Top, b.Top),
                      Math.Max(a.Right, b.Right),
                      Math.Max(a.Bottom, b.Bottom)
                  );

                  newList.Add(mergedBox);
                  boundingBoxes.RemoveAt(j);
                  merged = true;
                  hasMerged = true;
                  break;
               }
            }

            if (!hasMerged)
               newList.Add(a);
         }

         boundingBoxes = newList;

      } while (merged);

      // -----------------------------
      // DRAW BOUNDING BOXES
      // -----------------------------
      using (var canvas = new SKCanvas(diff))
      {
         var paint = new SKPaint
         {
            StrokeWidth = 3,
            IsStroke = true
         };

         var colors = new[]
         {
                SKColors.Lime, SKColors.Cyan, SKColors.Magenta,
                SKColors.Yellow, SKColors.Orange, SKColors.Blue
            };

         for (int i = 0; i < boundingBoxes.Count; i++)
         {
            paint.Color = colors[i % colors.Length];
            canvas.DrawRect(boundingBoxes[i], paint);
         }
      }

      // -----------------------------
      // OPTIONAL: DRAW BOUNDING BOXES ON SECOND IMAGE
      // -----------------------------
      if (!string.IsNullOrEmpty(img2wDiffPath))
      {
         using var second = bmp2.Copy();
         using var canvas2 = new SKCanvas(second);

         var paint2 = new SKPaint
         {
            StrokeWidth = 5,
            IsStroke = true,
            Color = SKColors.Red,
         };

         foreach (var box in boundingBoxes)
            canvas2.DrawRect(box, paint2);

         using var img2 = SKImage.FromBitmap(second);
         using var data2 = img2.Encode(SKEncodedImageFormat.Png, 100);
         using var stream2 = File.OpenWrite(img2wDiffPath);
         data2.SaveTo(stream2);
      }

      // -----------------------------
      // SAVE DIFF IMAGE
      // -----------------------------
      if (!string.IsNullOrEmpty(diffOnlyPath))
      {
         using (var image = SKImage.FromBitmap(diff))
         using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
         using (var stream = File.OpenWrite(diffOnlyPath))
         {
            data.SaveTo(stream);
         }
      }

      // -----------------------------
      // RESULT
      // -----------------------------
      int totalPixels = width * height;
      double percentage = (double)changedPixels / totalPixels * 100.0;

      return new ImageDiffResult
      {
         ChangedPixels = changedPixels,
         Percentage = percentage,
         BoundingBoxes = boundingBoxes
      };
   }
}
