using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;

Console.WriteLine("Campus Equipment Borrowing System - Activity 1 demonstration");

var students = new InMemoryStudentRepository();
var equipment = new InMemoryEquipmentRepository();
var borrowings = new InMemoryBorrowingRepository();
var borrowEquipment = new BorrowEquipmentService(students, equipment, borrowings);

// Successful case: Juan borrows the available projector.
var successfulBorrow = await borrowEquipment.ExecuteAsync(studentId: 1, equipmentId: 1);
Console.WriteLine($"Success case: {(successfulBorrow.Success ? "PASS" : "FAIL")} - {successfulBorrow.Message}");

// Failure case: Maria is registered but is not allowed to borrow equipment.
var failedBorrow = await borrowEquipment.ExecuteAsync(studentId: 2, equipmentId: 3);
Console.WriteLine($"Failure case: {(!failedBorrow.Success ? "PASS" : "FAIL")} - {failedBorrow.Message}");

return successfulBorrow.Success && !failedBorrow.Success ? 0 : 1;
