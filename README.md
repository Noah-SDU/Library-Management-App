# Library-Management-App
Home Assignment 2

----------------------------------------
Borrowing Test
----------------------------------------
Steps:
1. Log in as Member
2. Go to Library Section
3. Select Book and Click "Borrow"

Expected Result:
- Book gets added to "My Loans" list

Actual Result:

PASS - Book was successfully added in My Loans, could not be borrowed again 


----------------------------------------
Return Book Test
----------------------------------------
Steps:
1. Log in as Member
2. Go to "My Loans" Section
3. Select Book and Click "Return Selected Book"

Expected Result:
- Book gets removed from "My Loans" list
- Book becomes available in the Library Section

Actual Result:

PASS - Book was Successfully removed from "My Loans", can be borrowed again


----------------------------------------
Add Book Test
----------------------------------------
Steps:
1. Log in as Librarian
2. Go to "Full Catalog" Section
3. Write information about the Book(Title, Author, ISBN, Copies) and click "Add Book"

Expected Result:
- Book gets added to the Library Section list

Actual Result:

PASS - Book gets added to Library Section, members can borrow and rate it


----------------------------------------
Remove Book Test
----------------------------------------
Steps:
1. Log in as Librarian
2. Go to "Full Catalog" Section
3. Select a Book and press "Delete Book"

Expected Result:
- Book gets deleted from the Library Section

Actual Result:

PASS - Book is successfully deleted from the Library Section, members can no longer see and borrow it


----------------------------------------
Loan Tracking Test
----------------------------------------
Steps:
1. Log in as Librarian
2. Go to "Active Loans" Section

Expected Result:
- Librarian is shown a catalog of the members who borrowed specific books

Actual Result:

PASS - Librarians can see a list of the users' borrowed books


----------------------------------------
Login Test
----------------------------------------
Steps:
1. Start application
2. Login as Member
3. Observe Member layout and interface
4. Press "Return To Login"
5. Login as Librarian
6. Observe Librarian layout and interface

Expected Result:
- Login as Member should send you to a layout that allows you to loan, return and rate books
- Login as Librarian should send you to a layout that allows you to add or delete books and observe active loans

Actual Result:

PASS - Members and Librarians are sent to the correct layout


----------------------------------------
Search/Filter Test
----------------------------------------
Steps:
1. Log in as Member
2. Go to Library Section
3. Press "Search Catalog" and search the specific book or author

Expected Result:
- Book of choice is shown while the rest are filtered out

Actual Result:

PASS - Book is shown when searched while the rest are omitted


----------------------------------------
Data Persistence Test
----------------------------------------
Steps:
1. Log in as Librarian
2. Go to "Full Catalog" Section
3. Write information about the Book and click "Add Book"
4. Press "Return To Login"
5. Login as Member
6. Go to Library Section
7. Observe the catalog for changes

Expected Result:
- New book is shown in the Library section and saved in JSON file

Actual Result:

PASS - Members can now see the new book, it exists in the JSON file 