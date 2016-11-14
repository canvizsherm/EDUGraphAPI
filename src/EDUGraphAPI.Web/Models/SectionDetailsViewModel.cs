﻿using Microsoft.Education.Data;
using Microsoft.Graph;

namespace EDUGraphAPI.Web.ViewModels
{
    public class SectionDetailsViewModel
    {
        public School School { get; set; }

        public Section Section { get; set; }

        public string UserDisplayName { get; set; }

        public Conversation[] Conversations { get; set; }

        public string SeeMoreConversationsUrl { get; set; }

        public DriveItem[] DriveItems { get; set; }

        public string SeeMoreFilesUrl { get; set; }
    }
}