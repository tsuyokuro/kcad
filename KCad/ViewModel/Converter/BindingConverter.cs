using Plotter.Controller;

namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
    public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
    public class ViewModeConverter : EnumBoolConverter<ViewModes> { }
}
