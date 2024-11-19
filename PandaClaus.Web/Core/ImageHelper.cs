namespace PandaClaus.Web.Core;

public static class ImageHelper
{
    public static string GetContentType(string image)
    {
        image = image.ToLower();
        var contentType = "application/octet-stream";
        if (image.EndsWith("jpg") || image.EndsWith("jpeg"))
        {
            contentType = "image/jpeg";
        }
        else if (image.EndsWith("png"))
        {
            contentType = "image/png";
        }
        else if (image.EndsWith("bmp"))
        {
            contentType = "image/bmp";
        }
        else if (image.EndsWith("tif") || image.EndsWith("tiff"))
        {
            contentType = "image/tiff";
        }

        return contentType;
    }
}
