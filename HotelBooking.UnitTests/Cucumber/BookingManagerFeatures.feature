Feature: Booking Creation

  # TC1: Valid Booking
  Scenario: TC1 - Valid Booking
    Given the booking start date is "2025-04-01"
    And the booking end date is "2025-04-03"
    And a room is available for the specified dates
    When the user creates the booking
    Then the booking should be created successfully

  # Test Case 2: No Room Available
  Scenario: TC2 - No Room Available
    Given the booking start date is "2025-04-02"
    And the booking end date is "2025-04-05"
    And no room is available for the specified dates
    When the user tries to create the booking
    Then the booking should not be created

  # Test Case 3: StartDate in Past
  Scenario: TC3 - StartDate in Past
    Given the booking start date is "2025-03-29"
    And the booking end date is "2025-04-03"
    When the user tries to create the booking
    Then the booking should throw an ArgumentException

  # Test Case 4: EndDate before StartDate
  Scenario: TC4 - EndDate before StartDate
    Given the booking start date is "2025-04-05"
    And the booking end date is "2025-04-02"
    When the user tries to create the booking
    Then the booking should throw an ArgumentException

  # Test Case 5: Booking Object Null
  Scenario: TC5 - Booking Object Null
    Given the booking object is null
    When the user tries to create the booking
    Then the booking should throw a NullReferenceException

  # Test Case 6: Earliest Valid Booking
  Scenario: TC6 - Earliest Valid Booking
    Given the booking start date is "2025-04-01"
    And the booking end date is "2025-04-02"
    And a room is available for the specified dates
    When the user creates the booking
    Then the booking should be created successfully

  # Test Case 7: Latest Valid Booking
  Scenario: TC7 - Latest Valid Booking
    Given the booking start date is "2026-04-01"
    And the booking end date is "2026-04-02"
    And a room is available for the specified dates
    When the user creates the booking
    Then the booking should be created successfully

  # Test Case 8: StartDate = Today, Invalid
  Scenario: TC8 - StartDate = Today, Invalid
    Given the booking start date is "2025-03-30"
    And the booking end date is "2025-04-02"
    When the user tries to create the booking
    Then the booking should throw an ArgumentException

  # Test Case 9: EndDate = StartDate, Invalid
  Scenario: TC9 - EndDate = StartDate, Invalid
    Given the booking start date is "2025-04-05"
    And the booking end date is "2025-04-05"
    When the user tries to create the booking
    Then the booking should throw an ArgumentException

  # Test Case 10: Only One Room Available
  Scenario: TC10 - Only One Room Available
    Given the booking start date is "2025-04-02"
    And the booking end date is "2025-04-04"
    And only 1 room is available for the specified dates
    When the user creates the booking
    Then the booking should be created successfully
