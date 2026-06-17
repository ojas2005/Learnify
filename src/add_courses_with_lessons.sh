#!/bin/bash
set -e

GW="http://localhost:5010"

echo "=== STEP 1: Login as Instructor ==="
TOKEN_RESP=$(curl -s -X POST "$GW/api/identity/api/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"instructor@learnify.com","password":"Instructor@1234"}')

TOKEN=$(echo "$TOKEN_RESP" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('accessToken',''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
  echo "Login failed. Trying to register a new instructor..."
  REG_RESP=$(curl -s -X POST "$GW/api/identity/api/accounts/register" \
    -H "Content-Type: application/json" \
    -d '{
      "displayName": "Test Instructor",
      "email": "instructor@learnify.com",
      "password": "Instructor@1234",
      "role": "Instructor"
    }')
  echo "Registration response: $REG_RESP"
  
  TOKEN_RESP=$(curl -s -X POST "$GW/api/identity/api/accounts/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"instructor@learnify.com","password":"Instructor@1234"}')
  TOKEN=$(echo "$TOKEN_RESP" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('accessToken',''))" 2>/dev/null)
fi

if [ -z "$TOKEN" ]; then
  echo "Still could not get token. Response: $TOKEN_RESP"
  exit 1
fi

echo "Token acquired."

# Function to create a course
create_course() {
  local title=$1
  local description=$2
  local category=$3
  
  COURSE_RESP=$(curl -s -X POST "$GW/api/courses/api/courses" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{
      \"title\": \"$title\",
      \"description\": \"$description\",
      \"category\": \"$category\",
      \"topic\": \"$category\",
      \"level\": 0,
      \"price\": 49.99
    }")
  
  COURSE_ID=$(echo "$COURSE_RESP" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('id',''))" 2>/dev/null)
  echo "$COURSE_ID"
}

# Function to add a lesson
add_lesson() {
  local course_id=$1
  local lesson_num=$2
  local total=$3
  
  curl -s -X POST "$GW/api/curriculum/api/curriculum/course/$course_id/lessons" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "{
      \"title\": \"Lesson $lesson_num: Course Topic $lesson_num\",
      \"body\": \"This is lesson $lesson_num of $total. In this lesson, we cover topic $lesson_num in detail.\",
      \"format\": 1,
      \"mediaUrl\": \"https://example.com/lesson$lesson_num.mp4\",
      \"durationMinutes\": $((10 + lesson_num * 5)),
      \"isPreviewable\": $([ $lesson_num -eq 1 ] && echo "true" || echo "false")
    }" | python3 -m json.tool
}

echo "=== STEP 2: Create Course 1 with 8 lessons ==="
COURSE1_ID=$(create_course "Advanced Web Development" "Learn modern web development with 8 comprehensive lessons covering HTML, CSS, JavaScript, and frameworks." "Technology")
echo "Created Course 1 with ID: $COURSE1_ID"

for i in {1..8}; do
  echo "Adding lesson $i to Course 1..."
  add_lesson "$COURSE1_ID" $i 8
done

echo "=== STEP 3: Create Course 2 with 9 lessons ==="
COURSE2_ID=$(create_course "Data Science Fundamentals" "Master data science with 9 lessons covering statistics, machine learning, and data visualization." "Data Science")
echo "Created Course 2 with ID: $COURSE2_ID"

for i in {1..9}; do
  echo "Adding lesson $i to Course 2..."
  add_lesson "$COURSE2_ID" $i 9
done

echo "=== STEP 4: Create Course 3 with 10 lessons ==="
COURSE3_ID=$(create_course "Mobile App Development" "Build mobile applications with 10 lessons covering iOS, Android, and cross-platform development." "Technology")
echo "Created Course 3 with ID: $COURSE3_ID"

for i in {1..10}; do
  echo "Adding lesson $i to Course 3..."
  add_lesson "$COURSE3_ID" $i 10
done

echo "=== Successfully created 3 courses with lessons ==="
echo "Course 1 (8 lessons): ID $COURSE1_ID"
echo "Course 2 (9 lessons): ID $COURSE2_ID"
echo "Course 3 (10 lessons): ID $COURSE3_ID"
