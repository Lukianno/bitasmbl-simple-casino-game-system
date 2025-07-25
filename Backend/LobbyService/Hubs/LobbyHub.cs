using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CasinoGame.Facade.Models;
using CasinoGame.WalletService;
using Google.Protobuf.WellKnownTypes;
using LobbyService.Interfaces;
using Microsoft.AspNetCore.SignalR;


namespace LobbyService;

public interface ILobbyClient
{
    Task GameTableCreated(GameTable table);
    Task JoinRequestSent(Guid tableId);
    Task JoinApproved(GameTable table);
    Task BalanceFilled(Transaction transaction, double userWalletBalance);
    Task GameConcluded(GameResult gameResult);
    Task TransactionAdded(Transaction transaction);
    Task GameCanceled(Guid tableId);
}

public class LobbyHub(IGameTableManager gameTableManager) : Hub<ILobbyClient>
{
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("we are on connected state");
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            UserConnections[userId] = Context.ConnectionId;
            Console.WriteLine($"User {userId} connected with ID {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }



    public async Task SendJoinRequest(Guid tableId, string jwtToken)
    {
        var result = gameTableManager.SendJoinRequest(tableId, jwtToken);

        var gametable = gameTableManager.GetTableInformation(tableId);

        if (result)
            Console.WriteLine("Player sent join request!!!");

        await Clients.Client(gametable.CreatedBy).JoinRequestSent(tableId);
    }

    public async Task<GameTable> ApproveJoin(Guid tableId, string player)
    {
        Console.WriteLine($"[ApproveJoin] Approving join for player ID: {player} on table ID: {tableId}");

        var table = gameTableManager.ApproveJoin(tableId, player)
                    ?? throw new Exception("table null");

        if (table?.Players != null && table.Players.Count >= 2)
        {
            UserConnections.TryGetValue(table.Players[0], out var playerOneConnectionId);
            UserConnections.TryGetValue(table.Players[1], out var playerTwoConnectionId);

            if (!string.IsNullOrEmpty(playerOneConnectionId) && !string.IsNullOrEmpty(playerTwoConnectionId))
            {
                await Clients.Clients(playerOneConnectionId, playerTwoConnectionId)
                    .JoinApproved(table);
            }
        }
        else
        {
            Console.WriteLine("[ApproveJoin] Table.Players is null or has fewer than 2 players.");
        }

        return table;
    }

 
    public async Task<List<Transaction>> GetUserTransactions(string jwtToken)
        => await gameTableManager.GetUserTransactions(jwtToken);

    public async Task FillUpBalance(decimal amount, string jwtToken)
    {
        var transaction = await gameTableManager.FillUpBalance(amount, jwtToken);

        var userWallet = await gameTableManager.GetUserBalance(jwtToken);

        await Clients.User(transaction.UserId).BalanceFilled(transaction, userWallet.Balance);
    }

    public async Task RollbackTransaction(string transactionId, string jwtToken)
    {
        var transaction = await gameTableManager.RollbackTransaction(transactionId, jwtToken);

        await Clients.User(transaction.UserId).TransactionAdded(transaction);
    }

    public async Task CancelGame(Guid tableId)
    {
        var refundResult = gameTableManager.CancelTable(tableId);
        await Clients.All.GameCanceled(tableId);
    }
}
