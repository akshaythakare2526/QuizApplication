# Custom Quiz Feature - Setup Instructions

## Overview
The custom quiz feature has been successfully implemented! This allows users and admins to create their own quizzes, assign them to specific users, and track completion.

## What's Been Added

### 1. **Database Models** (`Models/CustomQuizzes.cs`)
- `CustomQuizzes` - Main quiz table
- `CustomQuizQuestions` - Junction table for quiz-question relationships
- `CustomQuizAssignments` - Tracks which users are assigned to which quizzes

### 2. **Controllers** (`Controllers/CustomQuizController.cs`)
Complete CRUD operations for custom quizzes:
- Create new quizzes
- Select questions for quizzes
- Assign quizzes to users
- View assigned quizzes
- View public quizzes
- Delete quizzes

### 3. **ViewModels** (`ViewModels/CustomQuizViewModels.cs`)
- CreateCustomQuizViewModel
- SelectQuestionsViewModel
- AssignQuizViewModel
- CustomQuizListViewModel
- CustomQuizDetailsViewModel
- And more...

### 4. **Views** (`Views/CustomQuiz/`)
- `Create.cshtml` - Create new quiz
- `MyQuizzes.cshtml` - View your created quizzes
- `AssignedQuizzes.cshtml` - View quizzes assigned to you
- `PublicQuizzes.cshtml` - Browse public quizzes
- `SelectQuestions.cshtml` - Select questions for quiz
- `AssignUsers.cshtml` - Assign quiz to users
- `Details.cshtml` - View quiz details
- `Delete.cshtml` - Delete confirmation

### 5. **Navigation Updates** (`Views/Shared/_Layout.cshtml`)
Added dropdown menus for:
- **Users**: Create Quiz, My Quizzes, Assigned to Me, Public Quizzes
- **Admins**: Access to custom quizzes in admin menu

### 6. **Dashboard Integration** (`Controllers/UsersController.cs` & `Views/Users/UserDashboard.cshtml`)
- Shows notifications for newly assigned quizzes
- Displays count of pending and completed custom quizzes
- Animated bell icon for new assignments

## Next Steps - Database Migration

**IMPORTANT:** You need to run the database migration to create the new tables.

### Step 1: Stop the Running Application
Close any running instances of the application.

### Step 2: Run Migration Commands
Open PowerShell in the project directory and run:

```powershell
cd D:\WebAPI\QuizApplication\QuizApplication
dotnet ef migrations add AddCustomQuizzesTables
dotnet ef database update
```

## How It Works

### For Regular Users:
1. **Create Custom Quiz**:
   - Navigate to "Custom Quizzes" → "Create Quiz"
   - Enter title, description, time limit, category, difficulty
   - Choose if quiz is public or private

2. **Select Questions**:
   - After creating, select questions from available pool
   - Questions can be filtered by category/difficulty

3. **Assign to Users** (Optional):
   - If not public, assign quiz to specific users
   - Assigned users get notifications on their dashboard

4. **Take Assigned Quizzes**:
   - View assigned quizzes in "Assigned to Me"
   - New quizzes show with "NEW!" badge
   - Complete quiz and see results

5. **Browse Public Quizzes**:
   - Anyone can take public quizzes
   - No assignment needed

### For Admins:
- Same capabilities as users
- Can also access through Admin menu
- Can delete any custom quiz
- Can view all quizzes

## Features Implemented

✅ **Create Custom Quizzes** - Users create their own quizzes with custom settings
✅ **Select Questions** - Choose specific questions from the question bank
✅ **Public/Private Mode** - Make quizzes public or assign to specific users
✅ **Assignment System** - Assign quizzes to specific users
✅ **Dashboard Notifications** - Users see new quiz assignments with alerts
✅ **Completion Tracking** - Track who completed the quiz and their scores
✅ **Statistics** - View completion rates and scores
✅ **Responsive Design** - All views are mobile-friendly
✅ **Admin Access** - Admins can create and manage all quizzes

## Database Schema

### CustomQuizzes Table
- CustomQuizId (PK)
- Title, Description
- CreatedByUserId (FK to Users)
- TimeLimit, CategoryId, DifficultyLevel
- IsPublic, IsActive, CreatedDate

### CustomQuizQuestions Table
- Id (PK)
- CustomQuizId (FK)
- QuestionId (FK)
- QuestionOrder

### CustomQuizAssignments Table
- AssignmentId (PK)
- CustomQuizId (FK)
- AssignedToUserId (FK)
- AssignedDate, CompletedDate
- IsCompleted, IsViewed, Score

## Testing Checklist

After running migrations, test the following:

1. ✓ Login as regular user
2. ✓ Create a new custom quiz
3. ✓ Select questions for the quiz
4. ✓ Assign quiz to another user
5. ✓ Login as that user
6. ✓ See notification on dashboard
7. ✓ View and take assigned quiz
8. ✓ Create a public quiz
9. ✓ Browse public quizzes
10. ✓ Login as admin and manage quizzes

## Notes

- The quiz creation process is intuitive with a step-by-step flow
- Users can only edit/delete their own quizzes (admins can delete any)
- Assignments cannot be removed once quiz is completed
- Public quizzes are visible to all users
- Dashboard shows real-time notifications for new assignments
- All views follow the existing gradient theme

## Support

If you encounter any issues:
1. Make sure migrations are applied
2. Check that all users have proper roles set
3. Verify database connections in appsettings.json
4. Check browser console for any JavaScript errors
