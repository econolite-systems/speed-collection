using AcyclicaService.Extensions;

namespace AcyclicaService.Test
{
    public class GooglePointsTests
    {
        [Fact]
        public void DecodeEncodeSegmentPolyline()
        {
            const string encodedPoints = "_pxbGpp{yNdArkAh@ni@FlI";

            var points = GooglePoints.Decode(encodedPoints).ToArray();
            if (points == null)
                throw new NullReferenceException("points");
            var actualEncodedPoints = GooglePoints.Encode(points);

            points.Should().NotBeNull();
            points.Should().HaveCount(4);
            actualEncodedPoints.Should().Be(encodedPoints);
        }

        [Fact]
        public void DecodeEncodeSegmentPolyline2()
        {
            const string encodedPoints = "_jybG`x|xNEwFOqDU_DQqDGgGSsWEcF?uCFaFpAsq@";

            var points = GooglePoints.Decode(encodedPoints).ToArray();
            if (points == null)
                throw new NullReferenceException("points");
            var actualEncodedPoints = GooglePoints.Encode(points);

            points.Should().NotBeNull();
            points.Should().HaveCount(11);
            actualEncodedPoints.Should().Be(encodedPoints);
        }

        [Fact]
        public void DecodeEncodeUnescapeSegmentPolyline()
        {
            const string encodedPoints = "}qxbGduwyN_DHeNVsT\\oFJsKLwOPiML";

            var points = GooglePoints.Decode(encodedPoints).ToArray();
            if (points == null)
                throw new NullReferenceException("points");
            var actualEncodedPoints = GooglePoints.Encode(points);

            points.Should().NotBeNull();
            points.Should().HaveCount(8);
            actualEncodedPoints.Should().Be(encodedPoints);
        }
    }
}
