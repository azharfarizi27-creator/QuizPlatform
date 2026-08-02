using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Context;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class FriendChatService : IFriendChatService
    {
        private readonly QuizDbContext context =
            new QuizDbContext();

        public List<UserSearchDto> SearchUsers(
            int currentUserId,
            string keyword
        )
        {
            keyword = keyword ?? "";

            var users = context.Users
                .Include(x => x.Role)
                .Where(x =>
                    x.Id != currentUserId &&
                    x.IsActive == true &&
                    (
                        x.FullName.Contains(keyword) ||
                        x.Username.Contains(keyword) ||
                        x.Email.Contains(keyword)
                    )
                )
                .OrderBy(x => x.FullName)
                .Take(30)
                .ToList();

            return users.Select(x => new UserSearchDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Username = x.Username,
                Email = x.Email,
                RoleName = x.Role != null ? x.Role.Name : "-",
                ProfileImage = x.ProfileImage,
                FriendStatus = GetFriendStatus(currentUserId, x.Id)
            }).ToList();
        }

        public void SendFriendRequest(
            int currentUserId,
            int receiverId
        )
        {
            if (currentUserId == receiverId)
                throw new Exception("Tidak bisa menambahkan diri sendiri");

            var receiver = context.Users
                .FirstOrDefault(x =>
                    x.Id == receiverId &&
                    x.IsActive == true
                );

            if (receiver == null)
                throw new Exception("User tujuan tidak ditemukan");

            var existing = context.FriendRequests
                .FirstOrDefault(x =>
                    (
                        x.RequesterId == currentUserId &&
                        x.ReceiverId == receiverId
                    )
                    ||
                    (
                        x.RequesterId == receiverId &&
                        x.ReceiverId == currentUserId
                    )
                );

            if (existing != null)
            {
                if (existing.Status == "Accepted")
                    throw new Exception("User ini sudah menjadi teman");

                if (existing.Status == "Pending")
                    throw new Exception("Request pertemanan masih pending");

                existing.RequesterId = currentUserId;
                existing.ReceiverId = receiverId;
                existing.Status = "Pending";
                existing.CreatedAt = DateTime.Now;
                existing.RespondedAt = null;

                context.SaveChanges();
                return;
            }

            var request = new FriendRequest
            {
                RequesterId = currentUserId,
                ReceiverId = receiverId,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                RespondedAt = null
            };

            context.FriendRequests.Add(request);
            context.SaveChanges();
        }

        public List<FriendRequestDto> GetPendingRequests(
            int currentUserId
        )
        {
            var requests = context.FriendRequests
                .Where(x =>
                    x.ReceiverId == currentUserId &&
                    x.Status == "Pending"
                )
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return requests
                .Select(x => BuildFriendRequestDto(x))
                .ToList();
        }

        public void AcceptFriendRequest(
            int currentUserId,
            int requestId
        )
        {
            var request = context.FriendRequests
                .FirstOrDefault(x =>
                    x.Id == requestId &&
                    x.ReceiverId == currentUserId &&
                    x.Status == "Pending"
                );

            if (request == null)
                throw new Exception("Request pertemanan tidak ditemukan");

            request.Status = "Accepted";
            request.RespondedAt = DateTime.Now;

            context.SaveChanges();
        }

        public void RejectFriendRequest(
            int currentUserId,
            int requestId
        )
        {
            var request = context.FriendRequests
                .FirstOrDefault(x =>
                    x.Id == requestId &&
                    x.ReceiverId == currentUserId &&
                    x.Status == "Pending"
                );

            if (request == null)
                throw new Exception("Request pertemanan tidak ditemukan");

            request.Status = "Rejected";
            request.RespondedAt = DateTime.Now;

            context.SaveChanges();
        }

        public List<FriendDto> GetMyFriends(
      int currentUserId
  )
        {
            var friendIds = context.FriendRequests
                .Where(x =>
                    x.Status == "Accepted" &&
                    (
                        x.RequesterId == currentUserId ||
                        x.ReceiverId == currentUserId
                    )
                )
                .Select(x =>
                    x.RequesterId == currentUserId
                        ? x.ReceiverId
                        : x.RequesterId
                )
                .ToList();

            var friends = context.Users
                .Include(x => x.Role)
                .Where(x => friendIds.Contains(x.Id))
                .OrderBy(x => x.FullName)
                .ToList();

            var result = new List<FriendDto>();

            foreach (var friend in friends)
            {
                var lastMessage = context.ChatMessages
                    .Where(x =>
                        (
                            x.SenderId == currentUserId &&
                            x.ReceiverId == friend.Id
                        )
                        ||
                        (
                            x.SenderId == friend.Id &&
                            x.ReceiverId == currentUserId
                        )
                    )
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                var unreadCount = context.ChatMessages
                    .Count(x =>
                        x.SenderId == friend.Id &&
                        x.ReceiverId == currentUserId &&
                        x.IsRead == false
                    );

                result.Add(new FriendDto
                {
                    UserId = friend.Id,
                    FullName = friend.FullName,
                    Username = friend.Username,
                    Email = friend.Email,
                    RoleName = friend.Role != null ? friend.Role.Name : "-",
                    ProfileImage = friend.ProfileImage,
                    UnreadCount = unreadCount,
                    LastMessageText =
                        lastMessage != null
                            ? lastMessage.MessageText
                            : "",
                    LastMessageAt =
                        lastMessage != null
                            ? (DateTime?)lastMessage.CreatedAt
                            : null
                });
            }

            return result
                .OrderByDescending(x => x.LastMessageAt)
                .ThenBy(x => x.FullName)
                .ToList();
        }

        public List<ChatMessageDto> GetConversation(
            int currentUserId,
            int friendId
        )
        {
            if (!AreFriends(currentUserId, friendId))
                throw new Exception("Kamu hanya bisa chat dengan teman");

            var unreadMessages = context.ChatMessages
                .Where(x =>
                    x.SenderId == friendId &&
                    x.ReceiverId == currentUserId &&
                    x.IsRead == false
                )
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            context.SaveChanges();

            var messages = context.ChatMessages
                .Where(x =>
                    (
                        x.SenderId == currentUserId &&
                        x.ReceiverId == friendId
                    )
                    ||
                    (
                        x.SenderId == friendId &&
                        x.ReceiverId == currentUserId
                    )
                )
                .OrderBy(x => x.CreatedAt)
                .ToList();

            return messages
                .Select(x => BuildChatMessageDto(x, currentUserId))
                .ToList();
        }

        public ChatMessageDto SendMessage(
            int currentUserId,
            SendChatMessageDto request
        )
        {
            if (request == null)
                throw new Exception("Data pesan kosong");

            if (request.ReceiverId <= 0)
                throw new Exception("Receiver tidak valid");

            if (string.IsNullOrWhiteSpace(request.MessageText))
                throw new Exception("Pesan tidak boleh kosong");

            if (!AreFriends(currentUserId, request.ReceiverId))
                throw new Exception("Kamu hanya bisa chat dengan teman");

            var message = new ChatMessage
            {
                SenderId = currentUserId,
                ReceiverId = request.ReceiverId,
                MessageText = request.MessageText.Trim(),
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            context.ChatMessages.Add(message);
            context.SaveChanges();

            return BuildChatMessageDto(message, currentUserId);
        }

        private bool AreFriends(
            int userId1,
            int userId2
        )
        {
            return context.FriendRequests.Any(x =>
                x.Status == "Accepted" &&
                (
                    (
                        x.RequesterId == userId1 &&
                        x.ReceiverId == userId2
                    )
                    ||
                    (
                        x.RequesterId == userId2 &&
                        x.ReceiverId == userId1
                    )
                )
            );
        }

        private string GetFriendStatus(
            int currentUserId,
            int targetUserId
        )
        {
            var request = context.FriendRequests
                .FirstOrDefault(x =>
                    (
                        x.RequesterId == currentUserId &&
                        x.ReceiverId == targetUserId
                    )
                    ||
                    (
                        x.RequesterId == targetUserId &&
                        x.ReceiverId == currentUserId
                    )
                );

            if (request == null)
                return "None";

            if (request.Status == "Accepted")
                return "Friend";

            if (request.Status == "Pending")
            {
                if (request.RequesterId == currentUserId)
                    return "PendingSent";

                return "PendingReceived";
            }

            return request.Status;
        }

        private FriendRequestDto BuildFriendRequestDto(
            FriendRequest request
        )
        {
            var requester = context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Id == request.RequesterId);

            var receiver = context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Id == request.ReceiverId);

            return new FriendRequestDto
            {
                Id = request.Id,

                RequesterId = request.RequesterId,
                RequesterName = requester != null ? requester.FullName : "-",
                RequesterUsername = requester != null ? requester.Username : "-",
                RequesterRole =
                    requester != null && requester.Role != null
                        ? requester.Role.Name
                        : "-",
                RequesterImage =
                    requester != null
                        ? requester.ProfileImage
                        : "",

                ReceiverId = request.ReceiverId,
                ReceiverName = receiver != null ? receiver.FullName : "-",
                ReceiverUsername = receiver != null ? receiver.Username : "-",
                ReceiverRole =
                    receiver != null && receiver.Role != null
                        ? receiver.Role.Name
                        : "-",
                ReceiverImage =
                    receiver != null
                        ? receiver.ProfileImage
                        : "",

                Status = request.Status,
                CreatedAt = request.CreatedAt,
                RespondedAt = request.RespondedAt
            };
        }

        private ChatMessageDto BuildChatMessageDto(
            ChatMessage message,
            int currentUserId
        )
        {
            var sender = context.Users
                .FirstOrDefault(x => x.Id == message.SenderId);

            var receiver = context.Users
                .FirstOrDefault(x => x.Id == message.ReceiverId);

            return new ChatMessageDto
            {
                Id = message.Id,

                SenderId = message.SenderId,
                SenderName = sender != null ? sender.FullName : "-",
                SenderImage = sender != null ? sender.ProfileImage : "",

                ReceiverId = message.ReceiverId,
                ReceiverName = receiver != null ? receiver.FullName : "-",
                ReceiverImage = receiver != null ? receiver.ProfileImage : "",

                MessageText = message.MessageText,
                IsMine = message.SenderId == currentUserId,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };
        }
    }
}