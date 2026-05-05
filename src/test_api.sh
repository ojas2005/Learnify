#!/bin/bash
set -e

GW="http://localhost:5010"

echo "=== STEP 1: Login as Instructor ==="
TOKEN_RESP=$(curl -s -X POST "$GW/api/identity/api/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"instructor@learnify.com","password":"Instructor@1234"}')

echo "Login response: $TOKEN_RESP"
TOKEN=$(echo "$TOKEN_RESP" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('accessToken',''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
  echo "=== Login failed, trying admin ==="
  TOKEN_RESP=$(curl -s -X POST "$GW/api/identity/api/accounts/login" \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@learnify.com","password":"Admin@1234"}')
  echo "Admin login response: $TOKEN_RESP"
  TOKEN=$(echo "$TOKEN_RESP" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('accessToken',''))" 2>/dev/null)
fi

if [ -z "$TOKEN" ]; then
  echo "=== Could not get token. Trying to list existing accounts ==="
  curl -s "$GW/api/identity/api/accounts/by-role/Instructor" | python3 -m json.tool 2>/dev/null
  exit 1
fi

echo ""
echo "=== Token acquired (first 60 chars): ${TOKEN:0:60}..."
echo ""

echo "=== STEP 2: Get available courses ==="
COURSES=$(curl -s "$GW/api/courses/api/courses" \
  -H "Authorization: Bearer $TOKEN")
echo "$COURSES" | python3 -m json.tool 2>/dev/null || echo "$COURSES"
COURSE_ID=$(echo "$COURSES" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d[0]['id'] if d else '')" 2>/dev/null)
echo ""
echo "Using course ID: $COURSE_ID"

echo ""
echo "=== STEP 3: Add lesson to course $COURSE_ID ==="
LESSON_RESP=$(curl -s -X POST "$GW/api/curriculum/api/curriculum/course/$COURSE_ID/lessons" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Introduction to the Course",
    "body": "Welcome! This lesson covers the basics.",
    "format": 1,
    "mediaUrl": "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
    "durationMinutes": 30,
    "isPreviewable": true
  }')
echo "Add lesson response:"
echo "$LESSON_RESP" | python3 -m json.tool 2>/dev/null || echo "$LESSON_RESP"
