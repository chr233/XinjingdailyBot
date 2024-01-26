using Microsoft.Extensions.Logging;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Helper;

/// <inheritdoc cref="IImageHelperService"/>
[AppService(typeof(IImageHelperService), LifeTime.Transient)]
public sealed class ImageHelperService(
    ILogger<ImageHelperService> _logger,
    ITelegramBotClient _botClient,
    ITextHelperService _textHelperService,
    IHttpHelperService _httpHelperService) : IImageHelperService
{
    /// <inheritdoc/>
    public async Task<string?> FuzzyImageCheck(Message message)
    {
        var handler = message.Type switch {
            MessageType.Photo when message.Photo != null => Task.FromResult(CalcPhotoRatio(message.Photo)),
            MessageType.Document when message.Document != null => CalcDocumentImageRatio(message, message.Document),

            _ => null
        };

        if (handler != null)
        {
            var ratio = await handler;

            if (ratio == null)
            {
                return null;
            }

            if (ratio < 0.3)
            {
                return "长图清晰度过低，请将其以文件模式发送，以切分此图片。\n\n在 PC 客户端上，拖入图片后取消 “压缩图片” 或 “图片格式” 选项即可以文件格式发送\n在 安卓 客户端上，长按发送按钮，点击文件图标即可以文件格式发送。";
            }
            if (ratio > 4.5)
            {
                return "图片过宽，建议将其以文件模式发送，以自动调整宽高比。";
            }
        }

        return null;
    }

    /// <summary>
    /// 检测Photo长宽比
    /// </summary>
    /// <param name="photoSizes"></param>
    /// <returns></returns>
    private double? CalcPhotoRatio(PhotoSize[] photoSizes)
    {
        _logger.LogWarning("{int}", photoSizes.Length);

        var size = photoSizes.FirstOrDefault();
        if (size == null)
        {
            return null;
        }

        var ratio = 1.0 * size.Width / size.Height;
        return ratio;
    }

    /// <summary>
    /// 检测Document长宽比 (如果是图片)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    private async Task<double?> CalcDocumentImageRatio(Message message, Document document)
    {
        if (document.MimeType == null || !document.MimeType.StartsWith("image/"))
        {
            return null;
        }

        var tipsMsg = await _botClient.AutoReplyAsync("正在处理图片文件，请稍候...", message);

        var file = await _botClient.GetFileAsync(document.FileId);
        var fileSteam = await _httpHelperService.GetTelegramFileHeader(file, 512);
        if (fileSteam == null)
        {
            _logger.LogWarning("文件流为 null");
            return null;
        }

        var fileReader = new BinaryReader(fileSteam);

        var size = document.MimeType switch {
            "image/gif" => DecodeGif(fileReader),
            "image/jpeg" => DecodeJfif(fileReader),
            "image/png" or
            "image/apng" => ParsePngFileHeader(fileReader),
            "image/webp" => null,
            "image/avif" => null,
            "image/svg+xml" => null,
            "image/bmp" => DecodeBitmap(fileReader),
            "image/tiff" => null,
            "image/x-icon" => null,
            _ => null,
        };

        if (size == null)
        {
            return null;
        }

        _logger.LogWarning("{type} 高度 {h} 宽度 {w}", document.MimeType, size.Value.Height, size.Value.Width);

        var ratio = 1.0 * size.Value.Width / size.Value.Height;
        return ratio;
    }


    /// <summary>
    /// 长图切图目标宽高比
    /// </summary>
    public double SplitTargetRatio = 9.0 / 12.0;

    /// <summary>
    /// 长图切图重复比例
    /// </summary>
    public double SplitRepetitionRatio = 0.03;

    /// <summary>
    /// 检查Document是否为图片, 以及是否模糊
    /// </summary>
    /// <param name="message"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    //private async Task<string?> SplitDocumentImage(Message message, Document document)
    //{
    //    if (document.MimeType == null || !document.MimeType.StartsWith("image/"))
    //    {
    //        return null;
    //    }

    //    var tipsMsg = await _botClient.AutoReplyAsync("正在处理，请稍候...", message);

    //    // 切分图像
    //    using var inputMs = new MemoryStream();
    //    var file = await _botClient.GetInfoAndDownloadFileAsync(document.FileId, inputMs);

    //    using var inputImgMs = new SKManagedStream(inputMs);
    //    var originBitmap = SKBitmap.Decode(inputImgMs);
    //    var originRatio = 1.0 * originBitmap.Width / originBitmap.Height;

    //    //长图自动切图
    //    if (originRatio < 0.4)
    //    {
    //        await _botClient.EditMessageTextAsync(tipsMsg, "检测到长图, 自动切图中...");

    //        var splitedImgs = new List<IAlbumInputMedia>();

    //        var splitMidHeight = (int)Math.Round(originBitmap.Width / SplitTargetRatio); // 每张高度（实际高度 midHeight + scanHeight * k, k∈[-1, 1]）
    //        var splitPadding = (int)(0.05 * splitMidHeight); // 每张上下重复高度
    //        var splitScanHeight = (int)(0.3 * splitMidHeight); // 上下扫描切分点高度
    //        var splicScanHorizontal = (int)(0.01 * originBitmap.Width); // 横向扫描距离

    //        var currentY = 0;
    //        while (currentY < originBitmap.Height)
    //        {
    //            var scanStartY = Math.Max(1, currentY + splitMidHeight - splitScanHeight);
    //            var scanEndY = Math.Min(originBitmap.Height, (currentY + splitMidHeight + splitScanHeight));

    //            var maxDiffY = 0;
    //            double maxDiff = -100;

    //            if (originBitmap.Height - currentY - splitPadding - splitScanHeight - splitMidHeight > 0)
    //            {
    //                for (var y = scanStartY; y < scanEndY; y++)
    //                {
    //                    double diff = 0;

    //                    for (var x = 0; x < originBitmap.Width; x++)
    //                    {
    //                        var p1 = originBitmap.GetPixel(x, y);

    //                        double minDiffPixel = 99999;
    //                        for (var qx = Math.Max(0, x - splicScanHorizontal); qx < Math.Min(x + splicScanHorizontal, originBitmap.Width - 1); qx++)
    //                        {
    //                            var p2 = originBitmap.GetPixel(qx, y - 1);
    //                            var diffPixel = Math.Sqrt(
    //                                Math.Pow(p1.Red - p2.Red, 2) +
    //                                Math.Pow(p1.Green - p2.Green, 2) +
    //                                Math.Pow(p1.Blue - p2.Blue, 2)
    //                            );

    //                            minDiffPixel = Math.Min(minDiffPixel, diffPixel);
    //                        }

    //                        diff += minDiffPixel;
    //                    }

    //                    if (diff > maxDiff)
    //                    {
    //                        maxDiff = diff;
    //                        maxDiffY = y;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                maxDiffY = originBitmap.Height;
    //            }
    //            var height = Math.Min(maxDiffY - currentY + splitPadding * (currentY == 0 ? 1 : 2), originBitmap.Height - currentY + splitPadding);


    //            var splitBitmap = new SKBitmap(originBitmap.Width, height);
    //            var canvas = new SKCanvas(splitBitmap);
    //            // Draw the portion of the original image onto the chunk
    //            //var srcRect = new SKRect(0, startY, originalBitmap.Width, startY + height);
    //            //var destRect = new SKRect(0, 0, originalBitmap.Width, height);
    //            //canvas.DrawBitmap(originBitmap, srcRect, destRect);
    //            canvas.Clear(SKColors.White);
    //            canvas.DrawBitmap(originBitmap, new SKPoint(0, (currentY == 0 ? 0 : -currentY + splitPadding)));
    //            canvas.Dispose();

    //            using var outputMs = new MemoryStream();
    //            using var outputImgMs = new SKManagedStream(outputMs);
    //            using var outputImg = SKImage.FromBitmap(splitBitmap);
    //            using var outputImgData = outputImg.Encode(SKEncodedImageFormat.Png, 100);
    //            outputImgData.SaveTo(outputMs);

    //            outputMs.Position = 0;
    //            splitedImgs.Add(new InputMediaPhoto(new InputFileStream(outputMs, $"split-{splitedImgs.Count}.png")));

    //            currentY = maxDiffY;

    //            if ()

    //        }

    //        for (var i = 0; i < Math.Ceiling((double)splitedImgs.Count / 9); i++)
    //        {
    //            await _botClient.SendMediaGroupAsync(msg.Chat, splitedImgs.Slice(i * 9, Math.Min(9, splitedImgs.Count - i * 9)), replyToMessageId: msg.MessageId);
    //        }

    //        inputMs.Close();
    //        originBitmap.Dispose();

    //        await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
    //        await _botClient.SendTextMessageAsync(msg.Chat, "图片切分处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
    //    }
    //    else if (originRatio > 2)
    //    {
    //        const double splitTargetRatio = 2; // 目标宽高比
    //        var targetHeight = (int)(originBitmap.Width / splitTargetRatio);
    //        var paintY = (int)(targetHeight / 2 - originBitmap.Height / 2);

    //        var img = new Bitmap(originBitmap.Width, targetHeight);
    //        var g = Graphics.FromImage(img);
    //        g.Clear(System.Drawing.Color.White);
    //        var imgBlurred = ConvolutionFilter(originBitmap, new double[,]
    //    { {  2, 04, 05, 04, 2 },
    //              {  4, 09, 12, 09, 4 },
    //              {  5, 12, 15, 12, 5 },
    //              {  4, 09, 12, 09, 4 },
    //              {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
    //        var imgBlurredBlurred = ConvolutionFilter(imgBlurred, new double[,]
    //    { {  2, 04, 05, 04, 2 },
    //              {  4, 09, 12, 09, 4 },
    //              {  5, 12, 15, 12, 5 },
    //              {  4, 09, 12, 09, 4 },
    //              {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
    //        var scale = targetHeight / originBitmap.Height * 1.6;
    //        g.DrawImage(imgBlurredBlurred, (int)((originBitmap.Width * scale - originBitmap.Width) * -0.5), (int)(-0.15 * targetHeight), (int)(originBitmap.Width * scale), (int)(originBitmap.Height * scale));
    //        g.DrawImage(originBitmap, new Point(0, paintY));
    //        g.Dispose();
    //        var memoryStream = new MemoryStream();
    //        img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
    //        img.Dispose();
    //        imgBlurred.Dispose();
    //        imgBlurredBlurred.Dispose();
    //        memoryStream.Position = 0;
    //        await _botClient.SendPhotoAsync(msg.Chat, new InputFileStream(memoryStream), replyToMessageId: msg.MessageId);
    //        await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
    //        await _botClient.SendTextMessageAsync(msg.Chat, "图片处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
    //    }
    //    return null;

    //    return null;
    //}


    ///// <inheritdoc/>
    //public async Task<bool> ProcessMessage(Message msg)
    //{
    //    return true;
    //}

    //// Taken from https://softwarebydefault.com/2013/06/09/image-blur-filters/
    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="sourceBitmap"></param>
    ///// <param name="filterMatrix"></param>
    ///// <param name="factor"></param>
    ///// <param name="bias"></param>
    ///// <returns></returns>
    //public static SKBitmap ConvolutionFilter(SKBitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0)
    //{
    //    var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
    //                             sourceBitmap.Width, sourceBitmap.Height),
    //                                               ImageLockMode.ReadOnly,
    //                                         System.Drawing.Imaging.PixelFormat.Format32bppArgb);


    //    var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
    //    var resultBuffer = new byte[sourceData.Stride * sourceData.Height];


    //    Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
    //    sourceBitmap.UnlockBits(sourceData);

    //    var blue = 0.0;
    //    var green = 0.0;
    //    var red = 0.0;

    //    var filterWidth = filterMatrix.GetLength(1);
    //    var filterHeight = filterMatrix.GetLength(0);

    //    var filterOffset = (filterWidth - 1) / 2;
    //    var calcOffset = 0;

    //    var byteOffset = 0;

    //    for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
    //    {
    //        for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
    //        {
    //            blue = 0;
    //            green = 0;
    //            red = 0;

    //            byteOffset = offsetY * sourceData.Stride + offsetX * 4;

    //            for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
    //            {
    //                for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
    //                {
    //                    calcOffset = byteOffset +
    //                                 (filterX * 4) +
    //                                 (filterY * sourceData.Stride);

    //                    blue += (double)(pixelBuffer[calcOffset]) *
    //                            filterMatrix[filterY + filterOffset,
    //                                                filterX + filterOffset];

    //                    green += (double)(pixelBuffer[calcOffset + 1]) *
    //                             filterMatrix[filterY + filterOffset,
    //                                                filterX + filterOffset];

    //                    red += (double)(pixelBuffer[calcOffset + 2]) *
    //                           filterMatrix[filterY + filterOffset,
    //                                              filterX + filterOffset];
    //                }
    //            }

    //            blue = factor * blue + bias;
    //            green = factor * green + bias;
    //            red = factor * red + bias;

    //            blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));

    //            green = (green > 255 ? 255 : (green < 0 ? 0 : green));

    //            red = (red > 255 ? 255 : (red < 0 ? 0 : red));

    //            const double brightness = 1 / 1.414;

    //            resultBuffer[byteOffset] = (byte)(blue * brightness);
    //            resultBuffer[byteOffset + 1] = (byte)(green * brightness);
    //            resultBuffer[byteOffset + 2] = (byte)(red * brightness);

    //            resultBuffer[byteOffset + 3] = 255;
    //        }
    //    }

    //    var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

    //    var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly,
    //                                         System.Drawing.Imaging.PixelFormat.Format32bppArgb);

    //    Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
    //    resultBitmap.UnlockBits(resultData);

    //    return resultBitmap;
    //}


    private Size? ParsePngFileHeader(BinaryReader reader)
    {
        // File header - check for the PNG signature.
        if (reader.ReadByte() != 0x89)
        {
            _logger.LogWarning("Png 文件头错误");
            return null;
        }

        // Skip the remaining header to reach the first chunk.
        _ = reader.ReadBytes(15);

        int width = reader.ReadInt32BE();
        int height = reader.ReadInt32BE();

        return new Size(width, height);
    }

    private Size DecodeBitmap(BinaryReader binaryReader)
    {
        binaryReader.ReadBytes(16);
        int width = binaryReader.ReadInt32();
        int height = binaryReader.ReadInt32();
        return new Size(width, height);
    }

    private Size DecodeGif(BinaryReader binaryReader)
    {
        int width = binaryReader.ReadInt16();
        int height = binaryReader.ReadInt16();
        return new Size(width, height);
    }

    private Size DecodeJfif(BinaryReader binaryReader)
    {
        while (binaryReader.ReadByte() == 0xff)
        {
            byte marker = binaryReader.ReadByte();
            short chunkLength = binaryReader.ReadInt16();
            if (marker == 0xc0)
            {
                binaryReader.ReadByte();
                int height = binaryReader.ReadInt16();
                int width = binaryReader.ReadInt16();
                return new Size(width, height);
            }

            if (chunkLength < 0)
            {
                ushort uchunkLength = (ushort)chunkLength;
                binaryReader.ReadBytes(uchunkLength - 2);
            }
            else
            {
                binaryReader.ReadBytes(chunkLength - 2);
            }
        }

        throw new ArgumentException(null, nameof(binaryReader));
    }
}
