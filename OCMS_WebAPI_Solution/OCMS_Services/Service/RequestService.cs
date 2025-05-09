﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Repositories.Repository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class RequestService : IRequestService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IUserRepository _userRepository;
        private readonly Lazy<ITrainingScheduleService> _trainingScheduleService;
        private readonly Lazy<ITrainingPlanService> _trainingPlanService;
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorAssignmentRepository _instructorAssignmentRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        public RequestService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            Lazy<ITrainingScheduleService> trainingScheduleService,
            Lazy<ITrainingPlanService> trainingPlanService, ITrainingScheduleRepository trainingScheduleRepository, ICourseRepository courseRepository, IInstructorAssignmentRepository instructorAssignmentRepository, ITraineeAssignRepository traineeAssignRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _candidateRepository = candidateRepository ?? throw new ArgumentNullException(nameof(candidateRepository));
            _trainingScheduleRepository = trainingScheduleRepository;
            _courseRepository = courseRepository;
            _instructorAssignmentRepository = instructorAssignmentRepository;
            _trainingScheduleService = trainingScheduleService ?? throw new ArgumentNullException(nameof(trainingScheduleService));
            _trainingPlanService = trainingPlanService ?? throw new ArgumentNullException(nameof(trainingPlanService));
            _traineeAssignRepository = traineeAssignRepository;
        }
        public RequestService(UnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IUserRepository userRepository, ICandidateRepository candidateRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
        }

        #region Create Request
        public async Task<Request> CreateRequestAsync(RequestDTO requestDto, string userId)
        {
            if (requestDto == null)
                throw new ArgumentNullException(nameof(requestDto));
            if (!Enum.IsDefined(typeof(RequestType), requestDto.RequestType))
                throw new ArgumentException($"Invalid RequestType: {requestDto.RequestType}");
            User user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

                bool isValidEntity = await ValidateRequestEntityIdAsync(requestDto.RequestType, requestDto.RequestEntityId);
                if (!isValidEntity)
                    throw new ArgumentException("Invalid RequestEntityId for the given RequestType.");
            
            var newRequest = new Request
            {
                RequestId = GenerateRequestId(),
                RequestUserId = userId,
                RequestUser= user,
                RequestEntityId = requestDto.RequestEntityId,
                Status=RequestStatus.Pending,
                RequestType = requestDto.RequestType,
                Description = requestDto.Description,
                Notes = requestDto.Notes,
                RequestDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ApprovedBy = null,
                ApprovedDate = null
            };

            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.SaveChangesAsync();

            // 🚀 Send notification to the director if NewPlan, RecurrentPlan, RelearnPlan
            if (newRequest.RequestType == RequestType.NewPlan ||
                newRequest.RequestType == RequestType.RecurrentPlan ||
                newRequest.RequestType == RequestType.RelearnPlan||
                newRequest.RequestType == RequestType.Update||
                newRequest.RequestType == RequestType.Delete||
                newRequest.RequestType== RequestType.AssignTrainee||
                newRequest.RequestType == RequestType.AddTraineeAssign||
                newRequest.RequestType== RequestType.SignRequest
                )
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    );
                }
            }
            if ( newRequest.RequestType == RequestType.SignRequest)
                
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} for certificateId {requestDto.RequestEntityId} need to be signed.",
                        "Request"
                    );
                }
            }
            else if(newRequest.RequestType == RequestType.CreateNew ||
                newRequest.RequestType == RequestType.CreateRecurrent ||
                newRequest.RequestType == RequestType.CreateRelearn)
            {
                
                var eduofficers = await _userRepository.GetUsersByRoleAsync("Training staff");
                
                    foreach (var edu in eduofficers)
                {
                    await _notificationService.SendNotificationAsync(
                        edu.UserId,
                        "New Request Submitted",
                        $"A new {newRequest.RequestType} request has been submitted for review.",
                        "Request"
                    );
                }
            }

            if (newRequest.RequestType == RequestType.CandidateImport)
            {
                var admins = await _userRepository.GetUsersByRoleAsync("Training staff");
                foreach (var admin in admins)
                {
                    await _notificationService.SendNotificationAsync(
                        admin.UserId,
                        "New Candidate Import Request",
                        "A new candidate import request has been submitted for review.",
                        "CandidateImport"
                    );
                }
            }

            if (newRequest.RequestType == RequestType.DecisionTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    );
                }
            }
            if (newRequest.RequestType == RequestType.CertificateTemplate)
            {
                var directors = await _userRepository.GetUsersByRoleAsync("HeadMaster");
                foreach (var director in directors)
                {
                    await _notificationService.SendNotificationAsync(
                        director.UserId,
                        "New Template Approval Request",
                        "A new template approval request has been submitted for review.",
                        "TemplateApprove"
                    );
                }
            }
            return newRequest;
        }
        #endregion

        #region Get All Requests
        public async Task<IEnumerable<RequestModel>> GetAllRequestsAsync()
        {
            var requests = await _unitOfWork.RequestRepository.GetAllAsync(); // Remove includeProperties
            

            return _mapper.Map<IEnumerable<RequestModel>>(requests);
        }
        #endregion

        #region Get Request By Id
        public async Task<RequestModel> GetRequestByIdAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId); // Remove includeProperties
           
            return _mapper.Map<RequestModel>(request);
        }
        #endregion

        public async Task<List<RequestModel>> GetRequestsForHeadMasterAsync()
        {
            var validRequestTypes = new[]
            {
        RequestType.NewPlan,
        RequestType.RelearnPlan,
        RequestType.RecurrentPlan,
        RequestType.AddTraineeAssign,
        RequestType.AssignTrainee,
        RequestType.DecisionTemplate,
        RequestType.CertificateTemplate,
        RequestType.PlanChange,
        RequestType.PlanDelete,
        RequestType.Update,
        RequestType.Delete,
        RequestType.SignRequest
    };

            var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                predicate: r => validRequestTypes.Contains(r.RequestType));

            return _mapper.Map<List<RequestModel>>(requests);
        }

        public async Task<List<RequestModel>> GetRequestsForEducationOfficerAsync()
        {
            var validRequestTypes = new[]
            {
        RequestType.CreateNew,
        RequestType.CreateRecurrent,
        RequestType.CreateRelearn,
        RequestType.Complaint,
        RequestType.CandidateImport
    };

            var requests = await _unitOfWork.RequestRepository.GetAllAsync(
                predicate: r => validRequestTypes.Contains(r.RequestType));

            return _mapper.Map<List<RequestModel>>(requests);
        }
        private string GenerateRequestId()
        {
            return $"REQ-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
        private async Task<bool> ValidateRequestEntityIdAsync(RequestType requestType, string entityId)
        {
            switch (requestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    var plan = await _unitOfWork.TrainingPlanRepository
                        .GetQueryable()
                        .Where(p => p.PlanId == entityId)
                        .Include(p => p.Courses)
                        .ThenInclude(c => c.Subjects)
                        .ThenInclude(s => s.Schedules)
                        .Include(p => p.Courses)
                        .ThenInclude(c => c.Trainees)
                         .FirstOrDefaultAsync();

                    if (plan == null)
                    {
                        throw new KeyNotFoundException("Training plan not found.");
                    }

                    // Step 1: Has at least one course
                    if (plan.Courses == null || !plan.Courses.Any())
                    {
                        throw new InvalidOperationException("Training plan must have at least one course.");
                    }

                    foreach (var course in plan.Courses)
                    {
                        // Step 2a: Each course has at least one subject
                        if (course.Subjects == null || !course.Subjects.Any())
                        {
                            throw new InvalidOperationException($"Course '{course.CourseName}' must have at least one subject.");
                        }

                        foreach (var subject in course.Subjects)
                        {
                            // Step 3: Each subject has at least one schedule
                            if (subject.Schedules == null || !subject.Schedules.Any())
                            {
                                throw new InvalidOperationException($"Subject '{subject.SubjectName}' in course '{course.CourseName}' must have at least one schedule.");
                            }
                        }
                    }

                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);

                case RequestType.Update:
                case RequestType.Delete:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    var validplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(entityId);
                    if (validplan != null && validplan.TrainingPlanStatus != TrainingPlanStatus.Approved)
                    {
                        throw new InvalidOperationException("The plan has not been approved yet you can update or delete it if needed.");
                    }

                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);
                case RequestType.PlanChange:
                case RequestType.PlanDelete:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    var trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(entityId);
                    if (trainingplan != null && trainingplan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        throw new InvalidOperationException("The request has already been approved and cannot send request.");
                    }

                    return await _unitOfWork.TrainingPlanRepository.ExistsAsync(tp => tp.PlanId == entityId);

                case RequestType.CreateNew:
                case RequestType.CreateRecurrent:
                case RequestType.CreateRelearn:
                    
                    return string.IsNullOrEmpty(entityId);

                case RequestType.Complaint:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;

                    return await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == entityId);

                case RequestType.DecisionTemplate:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.DecisionTemplateRepository.ExistsAsync(dt => dt.DecisionTemplateId == entityId);
                case RequestType.CertificateTemplate:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.CertificateTemplateRepository.ExistsAsync(dt => dt.CertificateTemplateId == entityId);
                case RequestType.SignRequest:
                    if (string.IsNullOrWhiteSpace(entityId))
                        return false;
                    return await _unitOfWork.CertificateRepository.ExistsAsync(dt => dt.CertificateId == entityId);
                        default:
                    return true;
            }
        }

        #region Delete Request
        public async Task<bool> DeleteRequestAsync(string requestId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return false;

            await _unitOfWork.RequestRepository.DeleteAsync(requestId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Approve Request
        public async Task<bool> ApproveRequestAsync(string requestId, string approvedByUserId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.ApprovedBy = approvedByUserId;
            request.ApprovedDate = DateTime.Now;
            request.UpdatedAt = DateTime.Now;
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Approved;
            }
            else if (request != null && request.Status == RequestStatus.Rejected)
            {
                throw new InvalidOperationException("The request has already been rejected and cannot be approved.");
            }
            else
            {
                throw new InvalidOperationException("The request cannot be approved in its current status.");
            }


            // Notify the requester
            await _notificationService.SendNotificationAsync(
                request.RequestUserId,
                "Request Approved",
                $"Your request ({request.RequestType}) has been approved.",
                "Request"
            );
            var approver = await _userRepository.GetByIdAsync(approvedByUserId);
            // Handle request type-specific actions
            switch (request.RequestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:
                    
                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                        plan.ApproveByUserId = approvedByUserId;
                        plan.ApproveDate = DateTime.Now;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);
                        // ✅ Approve all courses in the plan
                        var courses = await _courseRepository.GetCoursesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var course in courses)
                        {
                            course.Status = CourseStatus.Approved;
                            course.Progress = Progress.Ongoing;
                            course.ApproveByUserId= approvedByUserId;
                            course.ApprovalDate = DateTime.Now;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);
                        }

                        // ✅ Approve all schedules
                        var schedules = await _trainingScheduleRepository.GetSchedulesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var schedule in schedules)
                        {
                            schedule.Status = ScheduleStatus.Incoming;
                            schedule.ModifiedDate = DateTime.Now;
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        }

                        // ✅ Approve all instructor assignments
                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Approved;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                        }
                        
                        if (!string.IsNullOrEmpty(request.RequestUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Trainee Plan Approved",
                                $"Your request for {request.RequestEntityId} has been approved.",
                                $"{request.RequestType.ToString()}"
                            );
                        }
                    }
                    break;
                case RequestType.Update:
                case RequestType.Delete:

                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingplan != null)
                    {
                        trainingplan.TrainingPlanStatus = TrainingPlanStatus.Pending;
                        trainingplan.ApproveByUserId = approvedByUserId;
                        trainingplan.ApproveDate = DateTime.Now;
                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingplan);
                        // ✅ Pending all courses in the plan to update or delete
                        var courses = await _courseRepository.GetCoursesByTrainingPlanIdAsync(trainingplan.PlanId);
                        foreach (var course in courses)
                        {
                            course.Status = CourseStatus.Pending;
                            course.Progress = Progress.NotYet;
                            course.ApproveByUserId = approvedByUserId;
                            course.ApprovalDate = DateTime.Now;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);
                        }
                        foreach (var course in courses)
                        {
                            var courseTraineeAssigns = await _traineeAssignRepository.GetTraineeAssignmentsByCourseIdAsync(course.CourseId);
                            foreach (var assign in courseTraineeAssigns)
                            {
                                assign.RequestStatus = RequestStatus.Pending;
                                assign.ApprovalDate = DateTime.Now;
                                await _unitOfWork.TraineeAssignRepository.UpdateAsync(assign);
                            }
                        }
                        // ✅ Pending all schedules
                        var schedules = await _trainingScheduleRepository.GetSchedulesByTrainingPlanIdAsync(trainingplan.PlanId);
                        foreach (var schedule in schedules)
                        {
                            schedule.Status = ScheduleStatus.Pending;
                            schedule.ModifiedDate = DateTime.Now;
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        }

                        // ✅ Pending all instructor assignments
                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(trainingplan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Pending;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                        }

                        if (!string.IsNullOrEmpty(request.RequestUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                request.RequestUserId,
                                "Trainee Plan Approved",
                                $"Your request for {request.RequestEntityId} has been approved.",
                                $"{request.RequestType.ToString()}"
                            );
                        }
                    }
                    break;
                case RequestType.CandidateImport:
                    if (approver == null || approver.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("Only Training Staff can approve candidate import requests.");
                    }
                    var candidates = await _candidateRepository.GetCandidatesByImportRequestIdAsync(requestId);
                    if (candidates != null && candidates.Any())
                    {
                        foreach (var candidate in candidates)
                        {
                            candidate.CandidateStatus = CandidateStatus.Approved;
                            await _unitOfWork.CandidateRepository.UpdateAsync(candidate);
                        }
                    }
                    
                    var admins = await _userRepository.GetUsersByRoleAsync("Admin");
                    foreach (var admin in admins)
                    {
                        await _notificationService.SendNotificationAsync(
                            admin.UserId,
                            "Candidate Import Approved",
                            "The candidate import request has been approved. Please create user accounts for the new candidates.",
                            "CandidateImport"
                        );
                    }

                    break;
                case RequestType.AssignTrainee:

                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var traineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync(t => t.RequestId == requestId);
                    if (traineeAssigns == null || !traineeAssigns.Any())
                        return false;

                    foreach (var assign in traineeAssigns)
                    {
                        assign.RequestStatus = RequestStatus.Approved;
                        assign.ApprovalDate = DateTime.Now;
                        assign.ApproveByUserId = approvedByUserId;
                        await _unitOfWork.TraineeAssignRepository.UpdateAsync(assign);
                        
                        // ✅ Notify the trainee
                        await _notificationService.SendNotificationAsync(
                            assign.TraineeId,
                            "Trainee Assignment Approved",
                            $"You have been assigned to Course {assign.CourseId}.",
                            "TraineeAssign"
                        );

                        // ✅ Notify the assigner (AssignByUserId)
                        if (!string.IsNullOrEmpty(assign.AssignByUserId))
                        {
                            await _notificationService.SendNotificationAsync(
                                assign.AssignByUserId,
                                "Trainee Assignment Approved",
                                $"Your request to assign {assign.TraineeId} to Course {assign.CourseId} has been approved.",
                                "TraineeAssign"
                            );
                        }
                    }
                    
                    break;

                case RequestType.AddTraineeAssign:

                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(t => t.RequestId == requestId);
                    if (traineeAssign == null)
                        return false;

                    traineeAssign.RequestStatus = RequestStatus.Approved;
                    traineeAssign.ApprovalDate = DateTime.Now;
                    traineeAssign.ApproveByUserId = approvedByUserId;
                    await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);
                    
                    // ✅ Notify the trainee
                    await _notificationService.SendNotificationAsync(
                        traineeAssign.TraineeId,
                        "Trainee Assignment Approved",
                        $"You have been assigned to Course {traineeAssign.CourseId}.",
                        "TraineeAssign"
                    );

                    // ✅ Notify the assigner (AssignByUserId)
                    if (!string.IsNullOrEmpty(traineeAssign.AssignByUserId))
                    {
                        await _notificationService.SendNotificationAsync(
                            traineeAssign.AssignByUserId,
                            "Trainee Assignment Approved",
                            $"Your request to assign {traineeAssign.TraineeId} to Course {traineeAssign.CourseId} has been approved.",
                            "TraineeAssign"
                        );
                    }
                    
                    break;
                case RequestType.PlanChange:

                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlan != null && trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        // Extract proposed changes from Notes
                        if (request.Notes != null && request.Notes.Contains("Proposed changes:"))
                        {
                            var jsonStart = request.Notes.IndexOf("{");
                            var jsonEnd = request.Notes.LastIndexOf("}") + 1;
                            if (jsonStart >= 0 && jsonEnd > jsonStart)
                            {
                                var json = request.Notes.Substring(jsonStart, jsonEnd - jsonStart);
                                var dto = JsonSerializer.Deserialize<TrainingPlanDTO>(json);

                                // Apply the changes
                                trainingPlan.PlanName = dto.PlanName;
                                trainingPlan.Desciption = dto.Desciption;
                                trainingPlan.ModifyDate = DateTime.Now;
                                trainingPlan.CreateByUserId = request.RequestUserId; // Or approvedByUserId
                                trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                                await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingPlan);
                            }
                        }
                    }
                    
                    break;

                case RequestType.PlanDelete:

                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var trainingPlanToDelete = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlanToDelete != null)
                    {
                        await _trainingPlanService.Value.DeleteTrainingPlanAsync(request.RequestEntityId);
                    }

                    break;

                case RequestType.DecisionTemplate:
                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var template = await _unitOfWork.DecisionTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (template != null)
                    {
                        template.TemplateStatus = (int)TemplateStatus.Active;
                        await _unitOfWork.DecisionTemplateRepository.UpdateAsync(template);
                    }

                    break;
                case RequestType.CertificateTemplate:
                    if (approver == null || approver.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can approve this request.");
                    }
                    var certificateTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (certificateTemplate != null)
                    {
                        certificateTemplate.templateStatus = TemplateStatus.Active;
                        await _unitOfWork.CertificateTemplateRepository.UpdateAsync(certificateTemplate);
                    }

                    break;
            }
            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Reject Request
        public async Task<bool> RejectRequestAsync(string requestId, string rejectionReason, string rejectByUserId)
        {
            var request = await _unitOfWork.RequestRepository.GetByIdAsync(requestId);
            
            request.UpdatedAt = DateTime.Now;
            request.ApprovedBy = rejectByUserId;
            request.ApprovedDate = DateTime.Now;
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Rejected;
            }
            else if (request != null && request.Status == RequestStatus.Approved)
            {
                throw new InvalidOperationException("The request has already been approved and cannot be rejected.");
            }
            else
            {
                throw new InvalidOperationException("The request cannot be rejected in its current status.");
            }

            // Tailor notification message based on RequestType
            string notificationTitle = "Request Rejected";
            string notificationMessage;
            var rejecter = await _userRepository.GetByIdAsync(rejectByUserId);
            switch (request.RequestType)
            {
                case RequestType.NewPlan:
                case RequestType.RecurrentPlan:
                case RequestType.RelearnPlan:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var plan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (plan != null)
                    {
                        plan.TrainingPlanStatus = TrainingPlanStatus.Rejected;
                        plan.ApproveByUserId = null; // Clear approval details
                        plan.ApproveDate = null;

                        await _unitOfWork.TrainingPlanRepository.UpdateAsync(plan);

                        // ❌ Reject all associated courses
                        var courses = await _courseRepository.GetCoursesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var course in courses)
                        {
                            course.Status = CourseStatus.Rejected;
                            await _unitOfWork.CourseRepository.UpdateAsync(course);
                        }

                        // ❌ Reject all associated schedules
                        var schedules = await _trainingScheduleRepository.GetSchedulesByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var schedule in schedules)
                        {
                            schedule.Status = ScheduleStatus.Canceled; // Use Canceled instead of Rejected (if applicable)
                            await _unitOfWork.TrainingScheduleRepository.UpdateAsync(schedule);
                        }

                        // ❌ Reject all instructor assignments
                        var instructorAssignments = await _instructorAssignmentRepository.GetAssignmentsByTrainingPlanIdAsync(plan.PlanId);
                        foreach (var assignment in instructorAssignments)
                        {
                            assignment.RequestStatus = RequestStatus.Rejected;
                            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
                        }
                        
                    }

                    notificationMessage = $"Your training plan request (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                   
                    break;

                case RequestType.Update:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingPlan != null && trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                    }
                    
                    notificationMessage = $"Your request to update (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                   
                    break;
                case RequestType.Delete:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var _trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (_trainingPlan != null && _trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        _trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                    }
                    
                    notificationMessage = $"Your request to delete (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";
                    
                    break;
                case RequestType.PlanChange:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (trainingplan != null && trainingplan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        trainingplan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                    }

                    notificationMessage = $"Your request to update (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";

                    break;
                case RequestType.PlanDelete:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var _trainingplan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(request.RequestEntityId);
                    if (_trainingplan != null && _trainingplan.TrainingPlanStatus == TrainingPlanStatus.Approved)
                    {
                        _trainingplan.TrainingPlanStatus = TrainingPlanStatus.Approved;
                    }

                    notificationMessage = $"Your request to delete (ID: {request.RequestEntityId}) has been rejected. Reason: {rejectionReason}";

                    break;
                case RequestType.CandidateImport:

                    if (rejecter == null || rejecter.RoleId != 3)
                    {
                        throw new UnauthorizedAccessException("Only TrainingStaff can reject this request.");
                    }

                    notificationMessage = $"Your candidate import request has been rejected. Reason: {rejectionReason}";
                    
                    break;
                case RequestType.AssignTrainee:

                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }

                    notificationMessage = $"Your request to assign trainee import has been rejected. Reason:{rejectionReason}";
                    
                    break;
                case RequestType.AddTraineeAssign:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    notificationMessage = $"Your request to assign a trainee has been rejected. Reason: {rejectionReason}";
                    
                    break;
                case RequestType.DecisionTemplate:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var template = await _unitOfWork.DecisionTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (template != null)
                    {
                        template.TemplateStatus = (int)TemplateStatus.Inactive;
                        await _unitOfWork.DecisionTemplateRepository.UpdateAsync(template);
                    }

                    notificationMessage = $"Your request to approve the template has been rejected. Reason: {rejectionReason}";

                    break;
                case RequestType.CertificateTemplate:
                    if (rejecter == null || rejecter.RoleId != 2)
                    {
                        throw new UnauthorizedAccessException("Only HeadMaster can reject this request.");
                    }
                    var certiTemplate = await _unitOfWork.CertificateTemplateRepository.GetByIdAsync(request.RequestEntityId);
                    if (certiTemplate != null)
                    {
                        certiTemplate.templateStatus = (int)TemplateStatus.Inactive;
                        await _unitOfWork.CertificateTemplateRepository.UpdateAsync(certiTemplate);
                    }

                    notificationMessage = $"Your request to approve the template has been rejected. Reason: {rejectionReason}";

                    break;
                default:
                    
                    notificationMessage = $"Your request ({request.RequestType}) has been rejected. Reason: {rejectionReason}";
                    
                    break;
            }

            // Send notification to the request creator
            await _notificationService.SendNotificationAsync(
                request.RequestUserId,
                notificationTitle,
                notificationMessage,
                "Request"
            );

            // Additional logic for CandidateImport
            if (request.RequestType == RequestType.CandidateImport)
            {
                var candidates = await _candidateRepository.GetCandidatesByImportRequestIdAsync(requestId);
                if (candidates != null && candidates.Any())
                {
                    foreach (var candidate in candidates)
                    {
                        candidate.CandidateStatus = CandidateStatus.Rejected;
                        await _unitOfWork.CandidateRepository.UpdateAsync(candidate);
                    }
                }
                
                var hrs = await _userRepository.GetUsersByRoleAsync("HR");
                foreach (var hr in hrs)
                {
                    await _notificationService.SendNotificationAsync(
                        hr.UserId,
                        "Candidate Import Rejected",
                        $"The candidate import request has been rejected. Reason: {rejectionReason}",
                        "CandidateImport"
                    );
                }
            }
            await _unitOfWork.RequestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
