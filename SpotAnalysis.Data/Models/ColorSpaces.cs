namespace SpotAnalysis.Data.Models;

public readonly record struct Rgb(byte R, byte G, byte B);

public readonly record struct Hsl(double H, double S, double L);

public readonly record struct Oklab(double L, double A, double B);
