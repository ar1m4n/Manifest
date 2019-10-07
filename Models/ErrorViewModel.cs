using System;
using System.ComponentModel.DataAnnotations;
using Manifest.Data;

namespace Manifest.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ApplicationUserCommentModel
    {
        public string ToId { get; set; }
        public string FromId { get; set; }
        public string Comment { get; set; }
        public bool IsInOkRole { get; set; }

        public ApplicationUserCommentModel() { }
        public ApplicationUserCommentModel(ApplicationUserComment model, bool isInOkRole) {
            ToId = model.ToUserId;
            FromId = model.FromUserId;
            Comment = model.Comment;
            IsInOkRole = isInOkRole;
        }

    }
}
