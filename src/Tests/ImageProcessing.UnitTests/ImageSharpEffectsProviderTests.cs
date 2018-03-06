using System;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using ImageProcessing.Contracts;
using ImageProcessing.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Setting.Contracts;

namespace ImageProcessing.UnitTests
{
    [TestClass]
    public class ImageSharpEffectsProviderTests
    {
        private ILogger _logger = A.Fake<ILogger>();
        private IMetricReporter _metric = A.Fake<IMetricReporter>();

        private EffectPipelineExecuterBase _firstFakeExecuter = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerA;
        private EffectPipelineExecuterBase _executerB;
        private ResizeEffectParameters _resizeParameters = new ResizeEffectParameters(50, 60);

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            // arrange
            A.CallTo(() => _firstFakeExecuter.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored,
                                        A<IImmutableList<EffectPipelineExecuterBase>>.Ignored))
                .ReturnsLazily<Task, Stream, Stream>((input, output) =>
                                                    input.CopyToAsync(output));
        }

        #endregion // Setup

        #region ImageSharpEffectsProvider_Test

        [TestMethod]
        public async Task ImageSharpEffectsProvider_Test()
        {
            var effectProvider = new ImageSharpEffectsProvider(_metric, _logger);
            _executerA = effectProvider.CreateExecutor(null, _resizeParameters);
            _executerB = effectProvider.CreateExecutor(_executerA, GrayscaleEffectParameters.Default);

            using (var bmp = new Bitmap(100, 100))
            {
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.FillRectangle(Brushes.Red, 0, 0, 100, 100); // outer red rect
                    graphics.FillRectangle(Brushes.Green, 10, 10, 80, 80); // inner green rect
                }

                using (var inputStream = new MemoryStream())
                using (var outputStream = new MemoryStream())
                {
                    bmp.Save(inputStream, ImageFormat.Png);
                    inputStream.Position = 0;
                    await _executerB.ExecuteAsync(inputStream, outputStream);

                    Validation(inputStream, outputStream);
                }
            }
        }

        #endregion // ImageSharpEffectsProvider_Test


        #region Validation

        private void Validation(
            MemoryStream inputStream,
            MemoryStream outputStream)
        {
            inputStream.Position = 0;
            using (var affectedImage = Image.FromStream(outputStream))
            using (var affectedBitmap = new Bitmap(affectedImage))
            {
                // assert

                A.CallTo(() => _firstFakeExecuter.OnExecuteAsync(inputStream, A<Stream>.Ignored,
                                    A<IImmutableList<EffectPipelineExecuterBase>>.Ignored))
                    .MustHaveHappened(Repeated.Never);

                Assert.AreEqual(_resizeParameters.Width, affectedImage.Width);
                Assert.AreEqual(_resizeParameters.Height, affectedImage.Height);
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1).R < Color.Red.R);
                Assert.IsTrue(affectedBitmap.GetPixel(15, 15).G < Color.Green.G);
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1) == affectedBitmap.GetPixel(2, 2));
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1) != affectedBitmap.GetPixel(9, 9));
            }
        }

        #endregion // Validation
    }
}
