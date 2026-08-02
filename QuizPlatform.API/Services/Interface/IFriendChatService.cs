using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.DTO;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IFriendChatService
    {
        List<UserSearchDto> SearchUsers(int currentUserId, string keyword);

        void SendFriendRequest(int currentUserId, int receiverId);

        List<FriendRequestDto> GetPendingRequests(int currentUserId);

        void AcceptFriendRequest(int currentUserId, int requestId);

        void RejectFriendRequest(int currentUserId, int requestId);

        List<FriendDto> GetMyFriends(int currentUserId);

        List<ChatMessageDto> GetConversation(int currentUserId, int friendId);

        ChatMessageDto SendMessage(int currentUserId, SendChatMessageDto request);
    }
}