namespace VectorType.Models;

public class Particle
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public double X { get; set; }
    public double Y { get; set; }
    public double Vx { get; set; }
    public double Vy { get; set; }
    public string Color { get; set; } = "#00f0ff";
    public double Life { get; set; } = 1.0;
    public double MaxLife { get; set; } = 1.0;
    public double Size { get; set; } = 4.0;
    public string? TextChar { get; set; }

    public double Opacity => MaxLife > 0 ? Math.Clamp(Life / MaxLife, 0.0, 1.0) : 0;
}

public class LaserBeam
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public double StartX { get; set; } = 50.0;
    public double StartY { get; set; } = 92.0; // Turret defense line
    public double TargetX { get; set; }
    public double TargetY { get; set; }
    public string Color { get; set; } = "#00f0ff";
    public double Life { get; set; } = 0.25; // Dissipates in 250ms
    public double MaxLife { get; set; } = 0.25;

    public double Opacity => MaxLife > 0 ? Math.Clamp(Life / MaxLife, 0.0, 1.0) : 0;
}
