using BlackJack.Models;
using BlackJack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlackJack.Controllers
{
    [Route("blackjack")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        static GameStatus status = new GameStatus();
        private readonly DeckService _service;
        public DeckController(DeckService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<IActionResult> GetGame()
        {
            return Ok(status);
        }

        [HttpPost()]
        public async Task<IActionResult> NewGame()
        {
            DeckModel newDeck = await _service.NewDeck();
            status = new GameStatus();
            status.DeckId = newDeck.deck_id;
            DeckModel resultCards = await _service.DrawCards(3,status.DeckId);
            status.DealerCards = new List<Card>() { resultCards.cards[0] };
            status.PlayerCards = new List<Card>() { resultCards.cards[1], resultCards.cards[2] };
            status.DealerScore = GetCardScore(resultCards.cards[0]);
            status.PlayerScore = GetCardScore(resultCards.cards[1])+ GetCardScore(resultCards.cards[2]);
            status.GameOver = false;
            status.Outcome = "";

            if (status.PlayerScore > 21)
            {
                status.GameOver = true;
                status.Outcome = "Bust";
            }
            return Created("",status);
        }

        [HttpPost("play")]
        public async Task<IActionResult> GameAction(string action)
        {
            if(action == "hit")
            {
                DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                status.PlayerCards.Add(cardsResult.cards[0]);
                status.PlayerScore += GetCardScore(cardsResult.cards[0]);
                if(status.PlayerScore > 21)
                {
                    status.GameOver = true;
                    status.Outcome = "Bust";
                }
            }
            else if (action == "stand")
            {
                DeckModel cardsResult = await _service.DrawCards(1, status.DeckId);
                status.DealerCards.Add(cardsResult.cards[0]);
                status.DealerScore += GetCardScore(cardsResult.cards[0]);
            }
            return Ok(status); 
        }
            

        private int GetCardScore(Card c)
        {
            if(c.value == "ACE")
            {
                return 11;
            }
            else if(c.value == "KING" || c.value =="QUEEN" ||c.value == "JACK")
            {
                return 10;
            }
            else if(c.value == "JOKER")
            {
                return 0;
            }
            else
            {
                return int.Parse(c.value);
            }
        }
    }
}
