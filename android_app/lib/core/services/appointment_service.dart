import '../repositories/appointment_repository.dart';
import 'base_service.dart';

class AppointmentService extends BaseService {
  late AppointmentRepository _appointmentRepository;

  AppointmentService() {
    _appointmentRepository = AppointmentRepository();
  }

  /// Tạo appointment mới
  Future<Map<String, dynamic>> createAppointment({
    required String title,
    required DateTime startTime,
    required int reminderMinutes,
    String? description,
    String? location,
    List<String>? attendeeEmails,
    int? propertyId,
  }) async {
    final response = await _appointmentRepository.createAppointment(
      title: title,
      startTime: startTime,
      reminderMinutes: reminderMinutes,
      description: description,
      location: location,
      attendeeEmails: attendeeEmails,
      propertyId: propertyId,
    );
    return unwrapResponse(response);
  }

  /// Lấy danh sách appointments của user
  Future<List<Map<String, dynamic>>> getUserAppointments() async {
    final response = await _appointmentRepository.getUserAppointments();
    return unwrapListResponse(response);
  }

  /// Lấy danh sách appointments cho posts của user
  Future<List<Map<String, dynamic>>> getAllAppointmentsForMyPosts() async {
    final response = await _appointmentRepository.getAllAppointmentsForMyPosts();
    return unwrapListResponse(response);
  }

  /// Xác nhận appointment
  Future<Map<String, dynamic>> confirmAppointment(int appointmentId) async {
    final response = await _appointmentRepository.confirmAppointment(appointmentId);
    // Endpoint trả về success với data = null (no payload). Cho phép null data để không throw ApiException.
    return unwrapResponse(response, allowNullData: true) ?? <String, dynamic>{};
  }

  /// Từ chối appointment
  Future<Map<String, dynamic>> rejectAppointment(int appointmentId) async {
    final response = await _appointmentRepository.rejectAppointment(appointmentId);
    // Endpoint trả về success với data = null (no payload). Cho phép null data để không throw ApiException.
    return unwrapResponse(response, allowNullData: true) ?? <String, dynamic>{};
  }

  /// Hủy appointment (do người tạo hủy)
  Future<Map<String, dynamic>> cancelAppointment(int appointmentId) async {
    final response = await _appointmentRepository.cancelAppointment(appointmentId);
    return unwrapResponse(response, allowNullData: true) ?? <String, dynamic>{};
  }
}

