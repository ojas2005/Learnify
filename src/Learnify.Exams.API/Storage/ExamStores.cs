using Learnify.Core.Domain;
using Learnify.Exams.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Learnify.Exams.API.Storage;

// store for exams

public interface IExamStore
{
    Task<CourseExam?> GetByIdAsync(int examId);
    Task<List<CourseExam>> GetByCourseAsync(int courseId);
    Task<CourseExam> InsertExamAsync(CourseExam exam);
    Task<CourseExam> UpdateExamAsync(CourseExam exam);
    Task DeleteExamAsync(int examId);
}

public class ExamStore : IExamStore
{
    private readonly ExamsDbContext _db;
    public ExamStore(ExamsDbContext db)
    {
        _db = db;
    }

    public async Task<CourseExam?> GetByIdAsync(int examId)
    {
        return await _db.Exams.Include(e => e.Attempts).FirstOrDefaultAsync(e => e.Id == examId);
    }

    public async Task<List<CourseExam>> GetByCourseAsync(int courseId)
    {
        return await _db.Exams.Where(e => e.CourseId == courseId).ToListAsync();
    }

    public async Task<CourseExam> InsertExamAsync(CourseExam exam)
    {
        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();
        return exam;
    }

    public async Task<CourseExam> UpdateExamAsync(CourseExam exam)
    {
        _db.Exams.Update(exam);
        await _db.SaveChangesAsync();
        return exam;
    }

    public async Task DeleteExamAsync(int examId)
    {
        var exam = await _db.Exams.FindAsync(examId);
        if (exam is not null) { _db.Exams.Remove(exam); await _db.SaveChangesAsync(); }
    }
}

// store for exam attempts

public interface IAttemptStore
{
    Task<ExamAttempt?> GetByIdAsync(int attemptId);
    Task<List<ExamAttempt>> GetByLearnerAndExamAsync(int learnerId, int examId);
    Task<List<ExamAttempt>> GetAllForExamAsync(int examId);
    Task<int> CountAttemptsAsync(int learnerId, int examId);
    Task<ExamAttempt> CreateAttemptAsync(ExamAttempt attempt);
    Task<ExamAttempt> UpdateAttemptAsync(ExamAttempt attempt);
}

public class AttemptStore : IAttemptStore
{
    private readonly ExamsDbContext _db;
    public AttemptStore(ExamsDbContext db)
    {
        _db = db;
    }

    public async Task<ExamAttempt?> GetByIdAsync(int attemptId)
    {
        return await _db.Attempts.Include(a => a.Learner).FirstOrDefaultAsync(a => a.Id == attemptId);
    }

    public async Task<List<ExamAttempt>> GetByLearnerAndExamAsync(int learnerId, int examId)
    {
        return await _db.Attempts.Where(a => a.LearnerId == learnerId && a.ExamId == examId).OrderByDescending(a => a.BeganAt).ToListAsync();
    }

    public async Task<List<ExamAttempt>> GetAllForExamAsync(int examId)
    {
        return await _db.Attempts.Include(a => a.Learner).Where(a => a.ExamId == examId).OrderByDescending(a => a.BeganAt).ToListAsync();
    }

    public async Task<int> CountAttemptsAsync(int learnerId, int examId)
    {
        return await _db.Attempts.CountAsync(a => a.LearnerId == learnerId && a.ExamId == examId);
    }

    public async Task<ExamAttempt> CreateAttemptAsync(ExamAttempt attempt)
    {
        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync();
        return attempt;
    }

    public async Task<ExamAttempt> UpdateAttemptAsync(ExamAttempt attempt)
    {
        _db.Attempts.Update(attempt);
        await _db.SaveChangesAsync();
        return attempt;
    }
}
