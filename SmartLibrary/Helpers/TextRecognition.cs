using PaddleOCRSharp;

namespace SmartLibrary.Helpers
{
    public static class TextRecognition
    {
        private readonly static PaddleOCREngine ocr_engine = new();

        public static async ValueTask<List<string>> DetectTextAsync(string path)
        {
            List<string> strings = [];
            await Task.Run(() =>
            {
                OCRResult result = ocr_engine.DetectText(path);
                foreach (TextBlock textBlock in result.TextBlocks)
                {
                    strings.Add(textBlock.Text);
                }
            });
            return strings;
        }
    }
}
