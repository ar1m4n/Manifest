using System;
using System.Collections.Generic;
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
        public ApplicationUserCommentModel(ApplicationUserComment model, bool isInOkRole = true) {
            ToId = model.ToUserId;
            FromId = model.FromUserId;
            Comment = model.Comment;
            IsInOkRole = isInOkRole;
        }
    }

    public class ContractModel
    {
        public List<ApplicationUser> Users { get; set; }

        public string Date { get; set; }

        public string Address { get; set; }

        public string NameBG { get; set; }

        public string NameEN { get; set; }

        public string BzdrncCity { get; set; }

        public string BzdrncAddress { get; set; }
    }
}
