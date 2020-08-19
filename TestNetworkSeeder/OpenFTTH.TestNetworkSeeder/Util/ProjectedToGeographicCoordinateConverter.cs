using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.TestNetworkSeeder.Util
{
    public static class ProjectedToGeographicCoordinateConverter
    {
        static string _sourceCsWkt = null;
        static string _targetCsWkt = null;
        static IProjectedCoordinateSystem _fromCS;
        static IGeographicCoordinateSystem _toCS;
        static CoordinateTransformationFactory _ctfac;
        static ICoordinateTransformation _trans;

       

        public static double[] ConvertToGeographic(double x, double y)
        {
            Initialize();

            double[] fromPoint = new double[] { x, y };
            double[] toPoint = _trans.MathTransform.Transform(fromPoint);

            return toPoint;
        }


        private static void Initialize()
        {
            if (_fromCS == null)
            {
                // Initialize objects needed for coordinate transformation
                _fromCS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(_sourceCsWkt) as IProjectedCoordinateSystem;
                _toCS = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(_targetCsWkt) as IGeographicCoordinateSystem;
                _ctfac = new CoordinateTransformationFactory();
                _trans = _ctfac.CreateFromCoordinateSystems(_fromCS, _toCS);
            }
        }

    }
}
