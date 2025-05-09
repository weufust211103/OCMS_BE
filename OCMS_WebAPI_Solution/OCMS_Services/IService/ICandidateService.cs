﻿using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.IService
{
    public interface ICandidateService
    {
        Task<IEnumerable<Candidate>> GetAllCandidates();
        Task<Candidate> GetCandidateByIdAsync(string id);
        Task<IEnumerable<Candidate>> GetCandidatesByRequestIdAsync(string requestId);
        Task<ImportResult> ImportCandidatesFromExcelAsync(Stream fileStream, string importedByUserId, IBlobService blobService);
        Task<CandidateUpdateResponse> UpdateCandidateAsync(string id, CandidateUpdateDTO updatedCandidate);
        Task<(bool success, string message)> DeleteCandidateAsync(string id);
    }
}
