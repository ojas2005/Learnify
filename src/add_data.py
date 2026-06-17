import requests
import json

BASE_URL = "http://localhost:5010"
TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzIiwiZW1haWwiOiJhdXRob3JAbGVhcm5pZnkuY29tIiwidW5pcXVlX25hbWUiOiJDb3Vyc2UgQXV0aG9yIiwicm9sZSI6Ikluc3RydWN0b3IiLCJqdGkiOiJjZDhjZGIyMWE2ZDk0ZjNlOTg0MDZiZGM2YTE5ZDdmZiIsIm5iZiI6MTc3ODE1NTczMywiZXhwIjoxNzc4MjQyMTMzLCJpYXQiOjE3NzgxNTU3MzMsImlzcyI6ImxlYXJuaWZ5IiwiYXVkIjoibGVhcm5pZnktdXNlcnMifQ.PrUJchrkLgGNZp22xUuzNx5uAEezGyfOhg3Bu9Ta25I"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

def create_course(title, topic, lessons_count):
    print(f"Creating course: {title}")
    course_data = {
        "title": title,
        "synopsis": f"A comprehensive guide to {title}.",
        "topic": topic,
        "difficulty": 1, # Intermediate
        "language": "English",
        "listPrice": 49.99,
        "coverImageUrl": "https://placehold.co/600x400"
    }
    
    response = requests.post(f"{BASE_URL}/api/courses", headers=headers, json=course_data)
    if response.status_code != 201:
        print(f"Failed to create course: {response.status_code} - {response.text}")
        return None
    
    course = response.json()
    course_id = course['id']
    print(f"Course created with ID: {course_id}")
    
    for i in range(1, lessons_count + 1):
        lesson_data = {
            "title": f"Lesson {i}: Introduction to {title}",
            "body": f"This is the content for lesson {i} of {title}.",
            "format": 1, # Video
            "mediaUrl": f"https://example.com/video{i}.mp4",
            "durationMinutes": 15,
            "isPreviewable": i == 1
        }
        l_response = requests.post(f"{BASE_URL}/api/curriculum/course/{course_id}/lessons", headers=headers, json=lesson_data)
        if l_response.status_code == 201:
            print(f"  Added lesson {i}")
        else:
            print(f"  Failed to add lesson {i}: {l_response.text}")
    
    return course_id

if __name__ == "__main__":
    create_course("Advanced React Patterns", "Development", 8)
    create_course("Machine Learning Basics", "Data Science", 9)
    create_course("Cloud Architecture with Azure", "Cloud", 10)
