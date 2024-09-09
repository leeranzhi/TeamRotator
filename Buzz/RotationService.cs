namespace Buzz;

using System;
using System.Linq;

public class RotationService
{
    private readonly RotationDbContext _context;

    public RotationService(RotationDbContext context)
    {
        _context = context;
    }
}