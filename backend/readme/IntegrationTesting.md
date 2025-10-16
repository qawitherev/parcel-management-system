1. Test class starts
2. CustomWebApplicationFactory.InitializeAsync() called
   └── MySQL container starts
3. All tests in ParcelControllerTest run
4. Test class finishes
5. CustomWebApplicationFactory.DisposeAsync() called
   └── MySQL container disposed
6. Move to next test class (if any)