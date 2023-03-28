using System.Text.RegularExpressions;
using BublikHeadBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("6112065897:AAHLsnE4ZtQGSdz1BRq-at2sBzjbDB8aY08");
Users user = new Users();

using CancellationTokenSource cts = new ();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new ()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine(botClient.Timeout);
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

await MessageToConsole();

async Task MessageToConsole()
{
    int prost = 0; 

    Task.Run(() => { 
        while (true) 
        { 
            prost++; 
            if (prost >= 10) 
            { 
                prost = 0; 
            } 
            Console.WriteLine($"prost: {prost}"); 
            Task.Delay(15 * 60 * 1000).Wait(); 
        } 
    });
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    botClient.GetUpdatesAsync();
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    var userId = message.From.Username;
    
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {userId}.");

    bool userExist = false;
    foreach (var users in user.allUsersList)
    {
        if (users.Name == userId.ToString())
        {
            userExist = true;
            break;
        }
    }

    if (!userExist)
    {
        user.allUsersList.Add(
            new Users
            {
                Name = userId.ToString(),
                AmountOfTypesTypedBullshit = 0,
            });
        Console.WriteLine($"added user {userId}");
    }
    
    if (Regex.IsMatch(messageText.ToLowerInvariant(),@"(?i).*poroshek.*|(?i).*порошенк.*"))
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "loh",
            cancellationToken: cancellationToken);
    }

    if (Regex.IsMatch(messageText.ToLowerInvariant(),@"(?i).*zelensk.*|(?i).*зеленск.*|(?i).*зелю.*"))
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "top",
            cancellationToken: cancellationToken);
    }

    // if (Regex.IsMatch(messageText.ToLowerInvariant(),@"(?i).*юр.*|(?i).*юри.*"))
    // {
    //     Message sentMessage = await botClient.SendTextMessageAsync(
    //         chatId: chatId,
    //         text: "До речi, @itsCrea, де код",
    //         cancellationToken: cancellationToken);
    // }
    
    if (Regex.IsMatch(messageText.ToLowerInvariant(),@"(?i).*э.*"))
    {
        if (userExist)
        {
            foreach (var users in user.allUsersList)
            {
                if (users.Name == userId)
                {
                    users.AmountOfTypesTypedBullshit++;
                }

                if (users.AmountOfTypesTypedBullshit > 100)
                {
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        replyToMessageId: message.MessageId,
                        text: $" @{userId}, чому не державною",
                        cancellationToken: cancellationToken);

                    users.AmountOfTypesTypedBullshit = 0;
                }
            }
        }
    }
        
    // if (messageText.ToLowerInvariant().Contains("козіна") || messageText.ToLowerInvariant().Contains("козiн"))
    // {
    //     Message sentMessage = await botClient.SendTextMessageAsync(
    //         chatId: chatId,
    //         text: "знову цей малорос",
    //         cancellationToken: cancellationToken);
    // }
    // Echo received message text
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
