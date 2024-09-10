namespace Buzz
{
    public class RotationService
    {
        private readonly RotationDbContext _context;

        public RotationService(RotationDbContext context)
        {
            _context = context;
        }

        public List<string> GetRotationList()
        {
            var assignmentList = _context.TaskAssignments.ToList();

            var rotationList = assignmentList
                .GroupBy(a => new { a.TaskName, a.PersonName, a.Id })
                .Select(e => new
                {
                    TaskInfo = $"{e.Key.TaskName}: {e.Key.PersonName}", e.Key.Id
                })
                .OrderBy(x => x.Id) 
                .Select(x => x.TaskInfo)
                .ToList();

            return rotationList;
        }
    }
}