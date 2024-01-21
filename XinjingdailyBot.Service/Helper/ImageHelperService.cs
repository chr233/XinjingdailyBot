using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Helper;

/// <inheritdoc cref="IImageHelperService"/>
[AppService(typeof(IImageHelperService), LifeTime.Transient)]
public sealed class ImageHelperService(
    ILogger<ImageHelperService> _logger,
    ITelegramBotClient _botClient
    ) : IImageHelperService
{

    /// <inheritdoc/>
    public async Task<bool> ProcessMessage(Message msg)
    {
        if (msg.Photo != null)
        {
            var size = msg.Photo.Last();
            var ratio = ((double)size.Width) / size.Height;
            if (ratio < 0.3)
            {
                await _botClient.SendTextMessageAsync(msg.Chat, "长图清晰度过低，请将其以文件模式发送，以切分此图片。\n\n在 PC 客户端上，拖入图片后取消 “压缩图片” 或 “图片格式” 选项即可以文件格式发送\n在 安卓 客户端上，长按发送按钮，点击文件图标即可以文件格式发送。", replyToMessageId: msg.MessageId);
                return false;
            }

            if (ratio > 4.5)
            {
                await _botClient.SendTextMessageAsync(msg.Chat, "图片过宽，建议将其以文件模式发送，以自动调整宽高比。", replyToMessageId: msg.MessageId);
            }
        }

        if (msg.Document != null)
        {
            if (msg.Document.MimeType.StartsWith("image/"))
            {
                var tipsMsg = await _botClient.SendTextMessageAsync(msg.Chat, "正在处理，请稍候……", replyToMessageId: msg.MessageId);
                // 切分图像
                Stream fileStream = new MemoryStream();
                await _botClient.GetInfoAndDownloadFileAsync(msg.Document.FileId, fileStream);
                var originImg = new Bitmap(Image.FromStream(fileStream));
                var originRatio = originImg.Width / originImg.Height;
                if (originRatio < 0.4)
                {
                    // split image
                    var imgs = new List<IAlbumInputMedia>();
                    const double splitTargetRatio = 9.0 / 12.0; // 目标宽高比
                    var splitMidHeight = (int)Math.Round(originImg.Width / splitTargetRatio); // 每张高度（实际高度 midHeight + scanHeight * k, k∈[-1, 1]）
                    var splitPadding = (int)(0.05 * splitMidHeight); // 每张上下重复高度
                    var splitScanHeight = (int)(0.3 * splitMidHeight); // 上下扫描切分点高度
                    var splicScanHorizontal = (int)(0.01 * originImg.Width); // 横向扫描距离

                    var currentY = 0;
                    while (currentY < originImg.Height)
                    {
                        var scanStartY = Math.Max(1, currentY + splitMidHeight - splitScanHeight);
                        var scanEndY = Math.Min((int)originImg.Height, (currentY + splitMidHeight + splitScanHeight));

                        var maxDiffY = 0;
                        double maxDiff = -100;

                        if (originImg.Height - currentY - splitPadding - splitScanHeight - splitMidHeight > 0)
                            for (var y = scanStartY; y < scanEndY; y++)
                            {
                                double diff = 0;

                                for (var x = 0; x < originImg.Width; x++)
                                {
                                    var p1 = originImg.GetPixel(x, y);

                                    double minDiffPixel = 99999;
                                    for (var qx = Math.Max(0, x - splicScanHorizontal); qx < Math.Min(x + splicScanHorizontal, originImg.Width - 1); qx++)
                                    {
                                        var p2 = originImg.GetPixel(qx, y - 1);
                                        var diffPixel = Math.Sqrt(
                                            Math.Pow(p1.R - p2.R, 2) +
                                            Math.Pow(p1.G - p2.G, 2) +
                                            Math.Pow(p1.B - p2.B, 2)
                                            );

                                        minDiffPixel = Math.Min(minDiffPixel, diffPixel);
                                    }

                                    diff += minDiffPixel;
                                }

                                if (diff > maxDiff)
                                {
                                    maxDiff = diff;
                                    maxDiffY = y;
                                }
                            }
                        else maxDiffY = originImg.Height;
                        var height = Math.Min(maxDiffY - currentY + splitPadding * (currentY == 0 ? 1 : 2), originImg.Height - currentY + splitPadding);


                        var img = new Bitmap(originImg.Width, height);
                        var g = Graphics.FromImage(img);
                        g.Clear(System.Drawing.Color.White);
                        g.DrawImage(originImg, new Point(0, (currentY == 0 ? 0 : -currentY + splitPadding)));
                        g.Dispose();

                        var memoryStream = new MemoryStream();
                        img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        img.Dispose();
                        memoryStream.Position = 0;
                        imgs.Add(new InputMediaPhoto(new InputFileStream(memoryStream, $"image{imgs.Count}.png")));

                        currentY = maxDiffY;
                    }

                    for (var i = 0; i < Math.Ceiling((double)imgs.Count / 9); i++)
                    {
                        await _botClient.SendMediaGroupAsync(msg.Chat, imgs.Slice(i * 9, Math.Min(9, imgs.Count - i * 9)), replyToMessageId: msg.MessageId);
                    }

                    fileStream.Close();
                    originImg.Dispose();

                    await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
                    await _botClient.SendTextMessageAsync(msg.Chat, "图片切分处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
                }
                else if (originRatio > 2)
                {
                    const double splitTargetRatio = 2; // 目标宽高比
                    var targetHeight = (int)(originImg.Width / splitTargetRatio);
                    var paintY = (int)(targetHeight / 2 - originImg.Height / 2);

                    var img = new Bitmap(originImg.Width, targetHeight);
                    var g = Graphics.FromImage(img);
                    g.Clear(System.Drawing.Color.White);
                    var imgBlurred = ConvolutionFilter(originImg, new double[,]
                { {  2, 04, 05, 04, 2 },
                  {  4, 09, 12, 09, 4 },
                  {  5, 12, 15, 12, 5 },
                  {  4, 09, 12, 09, 4 },
                  {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
                    var imgBlurredBlurred = ConvolutionFilter(imgBlurred, new double[,]
                { {  2, 04, 05, 04, 2 },
                  {  4, 09, 12, 09, 4 },
                  {  5, 12, 15, 12, 5 },
                  {  4, 09, 12, 09, 4 },
                  {  2, 04, 05, 04, 2 }, }, 1.0 / 159.0);
                    var scale = targetHeight / originImg.Height * 1.6;
                    g.DrawImage(imgBlurredBlurred, (int)((originImg.Width * scale - originImg.Width) * -0.5), (int)(-0.15 * targetHeight), (int)(originImg.Width * scale), (int)(originImg.Height * scale));
                    g.DrawImage(originImg, new Point(0, paintY));
                    g.Dispose();
                    var memoryStream = new MemoryStream();
                    img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    img.Dispose();
                    imgBlurred.Dispose();
                    imgBlurredBlurred.Dispose();
                    memoryStream.Position = 0;
                    await _botClient.SendPhotoAsync(msg.Chat, new InputFileStream(memoryStream), replyToMessageId: msg.MessageId);
                    await _botClient.DeleteMessageAsync(tipsMsg.Chat, tipsMsg.MessageId);
                    await _botClient.SendTextMessageAsync(msg.Chat, "图片处理完成，请选择要投稿的图片并转发给机器人。", replyToMessageId: msg.MessageId);
                }
                return false;
            }
        }
        return true;
    }

    // Taken from https://softwarebydefault.com/2013/06/09/image-blur-filters/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <param name="filterMatrix"></param>
    /// <param name="factor"></param>
    /// <param name="bias"></param>
    /// <returns></returns>
    public static Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,] filterMatrix, double factor = 1, int bias = 0)
    {
        var sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                 sourceBitmap.Width, sourceBitmap.Height),
                                                   ImageLockMode.ReadOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);


        var pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
        var resultBuffer = new byte[sourceData.Stride * sourceData.Height];


        Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
        sourceBitmap.UnlockBits(sourceData);

        var blue = 0.0;
        var green = 0.0;
        var red = 0.0;

        var filterWidth = filterMatrix.GetLength(1);
        var filterHeight = filterMatrix.GetLength(0);

        var filterOffset = (filterWidth - 1) / 2;
        var calcOffset = 0;

        var byteOffset = 0;

        for (var offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
        {
            for (var offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
            {
                blue = 0;
                green = 0;
                red = 0;

                byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                for (var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                {
                    for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        calcOffset = byteOffset +
                                     (filterX * 4) +
                                     (filterY * sourceData.Stride);

                        blue += (double)(pixelBuffer[calcOffset]) *
                                filterMatrix[filterY + filterOffset,
                                                    filterX + filterOffset];

                        green += (double)(pixelBuffer[calcOffset + 1]) *
                                 filterMatrix[filterY + filterOffset,
                                                    filterX + filterOffset];

                        red += (double)(pixelBuffer[calcOffset + 2]) *
                               filterMatrix[filterY + filterOffset,
                                                  filterX + filterOffset];
                    }
                }

                blue = factor * blue + bias;
                green = factor * green + bias;
                red = factor * red + bias;

                blue = (blue > 255 ? 255 : (blue < 0 ? 0 : blue));

                green = (green > 255 ? 255 : (green < 0 ? 0 : green));

                red = (red > 255 ? 255 : (red < 0 ? 0 : red));

                const double brightness = 1 / 1.414;

                resultBuffer[byteOffset] = (byte)(blue * brightness);
                resultBuffer[byteOffset + 1] = (byte)(green * brightness);
                resultBuffer[byteOffset + 2] = (byte)(red * brightness);

                resultBuffer[byteOffset + 3] = 255;
            }
        }

        var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

        var resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
        resultBitmap.UnlockBits(resultData);

        return resultBitmap;
    }
}
