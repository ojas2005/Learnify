# Function to stop all background processes on exit
cleanup() {
    echo "Stopping all microservices..."
    kill $(jobs -p)
    exit
}

trap cleanup SIGINT SIGTERM

echo "Starting Learnify Microservices and API Gateway..."

# Start each microservice in the background
dotnet run --project Learnify.Identity.API &
dotnet run --project Learnify.Courses.API &
dotnet run --project Learnify.Curriculum.API &
dotnet run --project Learnify.Exams.API &
dotnet run --project Learnify.Registration.API &
dotnet run --project Learnify.Reviews.API &
dotnet run --project Learnify.Tracking.API &
dotnet run --project Learnify.Analytics.API &

# Wait a bit for services to start before starting the Gateway
echo "Waiting for microservices to initialize..."
sleep 10

echo "Starting API Gateway on http://localhost:5010..."
dotnet run --project Learnify.Gateway &

echo "All services are running. Press Ctrl+C to stop."
wait
