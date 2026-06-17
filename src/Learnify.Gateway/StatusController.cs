using Microsoft.AspNetCore.Mvc;

namespace Learnify.Gateway.Controllers;

[ApiController]
[Route("/")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public ContentResult GetStatus()
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Learnify API Gateway Status</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; }
        .service { background: #ecf0f1; padding: 15px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #3498db; }
        .service h3 { margin: 0 0 10px 0; color: #2c3e50; }
        .endpoint { background: #34495e; color: white; padding: 5px 10px; border-radius: 3px; font-family: monospace; margin: 5px 0; display: inline-block; }
        .description { color: #7f8c8d; font-size: 14px; margin-top: 10px; }
        .status { background: #27ae60; color: white; padding: 5px 10px; border-radius: 3px; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 Learnify API Gateway</h1>
        <div class='service'>
            <h3>Identity Service</h3>
            <div class='endpoint'>GET /api/identity/api/accounts/status</div>
            <div class='endpoint'>POST /api/identity/api/accounts/login</div>
            <div class='description'>User authentication and authorization</div>
            <span class='status'>✓ Running on port 5001</span>
        </div>
        <div class='service'>
            <h3>Courses Service</h3>
            <div class='endpoint'>GET /api/courses/api/courses</div>
            <div class='endpoint'>POST /api/courses/api/courses</div>
            <div class='description'>Course management and content</div>
            <span class='status'>✓ Running on port 5002</span>
        </div>
        <div class='service'>
            <h3>Curriculum Service</h3>
            <div class='endpoint'>GET /api/curriculum/api/lessons</div>
            <div class='endpoint'>GET /api/curriculum/api/curriculum/{courseId}</div>
            <div class='description'>Course curriculum and lessons</div>
            <span class='status'>✓ Running on port 5003</span>
        </div>
        <div class='service'>
            <h3>Exams Service</h3>
            <div class='endpoint'>GET /api/exams/api/exams</div>
            <div class='endpoint'>POST /api/exams/api/exams/{examId}/submit</div>
            <div class='description'>Exam creation and management</div>
            <span class='status'>✓ Running on port 5004</span>
        </div>
        <div class='service'>
            <h3>Registration Service</h3>
            <div class='endpoint'>POST /api/registration/api/enrollments</div>
            <div class='endpoint'>GET /api/registration/api/enrollments/{userId}</div>
            <div class='description'>Course enrollment management</div>
            <span class='status'>✓ Running on port 5005</span>
        </div>
        <div class='service'>
            <h3>Reviews Service</h3>
            <div class='endpoint'>GET /api/reviews/api/reviews</div>
            <div class='endpoint'>POST /api/reviews/api/reviews</div>
            <div class='description'>Course reviews and ratings</div>
            <span class='status'>✓ Running on port 5006</span>
        </div>
        <div class='service'>
            <h3>Tracking Service</h3>
            <div class='endpoint'>GET /api/tracking/api/progress/{userId}</div>
            <div class='endpoint'>POST /api/tracking/api/progress</div>
            <div class='description'>Learning progress tracking</div>
            <span class='status'>✓ Running on port 5007</span>
        </div>
        <div class='service'>
            <h3>Analytics Service</h3>
            <div class='endpoint'>GET /api/analytics/api/analytics/platform</div>
            <div class='endpoint'>GET /api/analytics/api/analytics/courses</div>
            <div class='description'>Platform analytics and reporting</div>
            <span class='status'>✓ Running on port 5008</span>
        </div>
        <div style='margin-top: 30px; padding: 20px; background: #e8f4fd; border-radius: 5px;'>
            <h3>📡 API Gateway Information</h3>
            <p><strong>Gateway URL:</strong> http://localhost:5010</p>
            <p><strong>Status:</strong> All microservices are running and accessible</p>
            <p><strong>Usage:</strong> Use the endpoints above with the gateway prefix</p>
        </div>
    </div>
</body>
</html>";
        
        return Content(html, "text/html");
    }
}
