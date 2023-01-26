using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Card
    {
        private CardValue _cardValue;
        public CardValue cardValue
        {
            get { return _cardValue; }
            set { _cardValue = value; }
        }
        private CardSuit _cardSuit;
        public CardSuit cardSuit
        {
            get { return _cardSuit; }
            set { _cardSuit = value; }
        }
        private int _cardStrength;
        public int cardStrength
        {
            get { return _cardStrength; }
            set { _cardStrength = value; }
        }
        public Card(CardValue cardValue, CardSuit cardSuit, int cardStrength)
        {
            this.cardValue = cardValue;
            this.cardSuit = cardSuit;
            this.cardStrength = cardStrength;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Card))
                return false;
            else
                return ((cardValue == cardValue) && (cardSuit == cardSuit) && (cardStrength == cardStrength));
        }
        public override int GetHashCode()
        {
            return ((int)cardValue * 17519) + ((int)cardSuit * 270371) + (cardStrength * 4166287);
        }
        public class PowerLevel
        {
            private FiveCardCombination _powerRating;
            public FiveCardCombination powerRating
            {
                get { return _powerRating; }
                set { _powerRating = value; }
            }
        }
    }
    public partial class Maincode
    {
        private static List<Card> deckOfCards = new List<Card>();  //Array of Lists With the LINQ Method https://www.delftstack.com/howto/csharp/csharp-array-of-lists/
        private static List<Card> p1_hand = new List<Card>();
        private static List<Card> p2_hand = new List<Card>();
        private static List<Card> p3_hand = new List<Card>();
        private static List<Card> p4_hand = new List<Card>();
        private static List<Card>[] playerHands = { p1_hand, p2_hand, p3_hand, p4_hand };
        private static List<Card> selectCards = new List<Card>();
        private static List<Card> lastPlayedCards = new List<Card>();
        private static int playerIndex;
        private static int passCount = 0;
        private static int roundCardFormat = 0; // When everybody passed, the last person chooses the how the next round is played. Play Singles, Pairs or 5 cards         
        public static void Main(string[] args)
        {
            Console.WriteLine("Launching Big 2 \n" + "\n" + "Loading... \n");
            CompileDeck();
            ShuffleDeck(deckOfCards);
            DealCards(deckOfCards);
            ArrangeCards();
            FindStartingPlayer(); //Output playerIndex
            while (playerHands[playerIndex].Count() != 0)
            {
                Console.WriteLine("player {0}, has...", playerIndex + 1);
                PrintHand(playerHands[playerIndex]);

            SelectCards:
                SelectCards(playerHands[playerIndex]);
                Console.WriteLine("Selected cards \n");
                PrintHand(selectCards);
                Console.WriteLine("\n");

                if (CheckSelectionLegality() == false) // Player tried to play an illegal card
                {
                    selectCards = new List<Card>();
                    goto SelectCards;
                }
                if (roundCardFormat == 0) // First played cards of the round determines the round's current card format.
                {
                    roundCardFormat = selectCards.Count();
                }
                List<Card> result = playerHands[playerIndex].Except(selectCards).ToList();
                Console.WriteLine("Player {0} remaining cards", playerIndex + 1);
                PrintHand(result);
                playerHands[playerIndex] = result;
                if (selectCards.Count != 0)
                {
                    lastPlayedCards = new List<Card>(selectCards);
                    passCount = 0;
                }
                selectCards.Clear();
                Console.WriteLine("{0} pass", passCount);
                if (passCount > 3) // When everyone pass, the last person wins the current round. He get to choose the next round's card format (Singles, Pairs or 5 cards combination)
                {
                    roundCardFormat = 0;
                    passCount = 0;
                    Console.WriteLine("Player {0} has won the current card round. \nPlayer {0}, please choose any your cards (Singles, Pairs or 5 cards combination) to play.\n", playerIndex + 1);
                    goto SelectCards;
                }
                if (playerHands[playerIndex].Count() == 0) //Win condition, hand have 0 cards
                {
                    break;
                }
                playerIndex++;
                if (playerIndex == 4)
                {
                    playerIndex = 0;
                }
            }
            Console.WriteLine("Player {0} has won the game.", playerIndex + 1);
            Console.ReadLine();
        }
        static bool CheckSelectionLegality()
        {
            if (selectCards.Count() != roundCardFormat && selectCards.Count() != 0 && roundCardFormat != 0) // Check if player plays too little or too much cards. Other 2 condition is an exception to Pass (No card selected) or new Round.
            {
                Console.WriteLine("This round, each player can only play {0} card. Please re-select your cards. \n", roundCardFormat);
                PrintHand(selectCards);
                Console.WriteLine("You have selected {0} cards. Please re-select your cards. \n", selectCards.Count());
                return false;
            }
            if (lastPlayedCards.Count() == 0 && lastPlayedCards.Count() == 0) //Start of the game, Three of DIAMOND has to be played
            {
                foreach (Card card in selectCards)
                {
                    if (card.cardValue == CardValue.Three && card.cardSuit == CardSuit.DIAMOND)
                        break;
                    else
                        Console.WriteLine("You are the first player of the game. \nYou need to play Three of DIAMOND. \n");
                    return false;
                }
            }
            if (selectCards.Count() == 0)
            {
                Console.WriteLine("Player {0} has passed the turn. \n", playerIndex + 1);
                return true;
            }
            if (playerHands.Sum(x => x.Count) == 52 && selectCards.Count() == 0) //Start of the game, but first player didnt throw a card. WHAT A TROLLER
            {
                Console.WriteLine("You are the first player of the game. \nYou need to play a card, specifically Three of DIAMOND. \n");
                Console.WriteLine("Game developer: Stop trying to break the game!\n");
                return false;
            }
            if (roundCardFormat == 0) // New Round, free to choose what to play (1,2 or 5 cards)
                return true;
            if (lastPlayedCards.Count() == 1) //  Check if the selected card is stronger than the last played card
            {
                if (lastPlayedCards[0].cardStrength < selectCards[0].cardStrength)
                    return true;
                else
                    return false;
            }
            if (lastPlayedCards.Count() == 2) // Check if the selected pair is bigger than the last played pair
            {
                if (selectCards[0].cardValue != selectCards[1].cardValue)
                {
                    Console.WriteLine("Playing doubles require 2 cards of the same card value. \nEtc Three of DIAMOND & Three of HEART.\n");
                    return false;
                }
                else if (Math.Max(selectCards[0].cardStrength, selectCards[1].cardStrength) > Math.Max(lastPlayedCards[0].cardStrength, lastPlayedCards[1].cardStrength))
                    return true;
                else
                    Console.WriteLine("The cards you've played does not beat the last played cards");
                return false;
            }
            if (selectCards.Count() == 3 || selectCards.Count() == 4) // Player did not enter a playable card format
            {
                Console.WriteLine("You can only play 1, 2 or 5 cards. Please re-select your cards. \n");
                PrintHand(selectCards);
                Console.WriteLine("You have selected {0} cards. Please re-select your cards. \n", selectCards.Count());
                return false;
            }
            if (selectCards.Count() == 5) // Check if player tried to play an invalid 5 card combination
            {
                if (CardCombinationRanking(selectCards) == FiveCardCombination.Null)
                {
                    Console.WriteLine("Please enter a valid 5 card combination.\n");
                    return false;
                }
            }
            if (lastPlayedCards.Count() == 5) // Check if the selected 5 card combination is stronger than the last played 5 card combination
            {
                if ((int)CardCombinationRanking(selectCards) > (int)CardCombinationRanking(lastPlayedCards))
                    return true;
                else if ((int)CardCombinationRanking(selectCards) == (int)CardCombinationRanking(lastPlayedCards))
                    if (CombinationStrength(selectCards, CardCombinationRanking(selectCards)) > CombinationStrength(lastPlayedCards, CardCombinationRanking(lastPlayedCards)))
                        return true;
                    else
                    {
                        Console.WriteLine("The cards you've played does not beat the last played cards");
                        return false;
                    }
                else
                    Console.WriteLine("The cards you've played does not beat the last played cards");
                return false;
            }
            Console.WriteLine("Congratulation, you found a bug :D.\n Please send a report to Zebo. \n"); // I must have missed out something, WHOOPS!
            return false;
        }
        private static int CombinationStrength(List<Card> cardList, FiveCardCombination combinationRank)
        {
            if (combinationRank == FiveCardCombination.FourOfAKind || combinationRank == FiveCardCombination.FullHouse)
                return (int)cardList.GroupBy(c => c.cardValue).OrderByDescending(group => group.Count()).First().Key;
            else if (combinationRank == FiveCardCombination.Flush)
                return (int)cardList.GroupBy(c => c.cardSuit).OrderByDescending(group => group.Count()).First().Key;
            if (combinationRank == FiveCardCombination.StraightFlush || combinationRank == FiveCardCombination.Straight)
                return (int)cardList.OrderByDescending(card => (int)(card.cardStrength)).Last().cardStrength;
            else // Should never happen
                return 0;
        }
        private static void CompileDeck()
        {
            Console.WriteLine("Assembling cards... OWO");
            int cardStrength = 0;
            foreach (CardValue cardValue in Enum.GetValues(typeof(CardValue)))
            {
                foreach (CardSuit cardSuit in Enum.GetValues(typeof(CardSuit)))
                {
                    cardStrength++;
                    deckOfCards.Add(new Card(cardValue, cardSuit, cardStrength));
                }
            }

            Console.WriteLine("Card Deck Assembled.");
        }
        private static void ShuffleDeck(List<Card> cardList)
        {
            int rangeOfList = cardList.Count;
            while (rangeOfList > 1)
            {
                rangeOfList--;
                int k = rng.Next(rangeOfList + 1);
                Card value = cardList[k];
                cardList[k] = cardList[rangeOfList];
                cardList[rangeOfList] = value;
            }
        }
        private static void DealCards(List<Card> deckOfCards)
        {
            Console.WriteLine("Dealing cards out... :3");
            int rangeOfList = deckOfCards.Count;
            while (rangeOfList > 1)
            {
                foreach (List<Card> playerHand in playerHands)
                {
                    playerHand.Add(deckOfCards[rangeOfList - 1]);
                    deckOfCards.RemoveAt(rangeOfList - 1);
                    rangeOfList--;
                }
            }
            Console.WriteLine("{0} cards to each player...", p4_hand.Count);
            Console.WriteLine("Good luck x3");
        }
        private static void ArrangeCards()
        {
            foreach (List<Card> playerHand in playerHands)
            {
                playerHand.Sort((x, y) => x.cardStrength.CompareTo(y.cardStrength));
            }
        }
        private static void FindStartingPlayer() //The player with Three of DIAMONDS start first
        {
            foreach (List<Card> playerHand in playerHands)
            {

                foreach (Card card in playerHand)
                {
                    if (card.cardValue == CardValue.Three && card.cardSuit == CardSuit.DIAMOND)
                    {
                        return;
                    }
                }
                playerIndex++;
            }
        }
        private static void PrintHand(List<Card> hand)
        {
            int i = 0;
            foreach (Card card in hand)
            {
                Console.WriteLine("Card {0} | Value: {1}, Subgroup: {2}", i + 1, card.cardValue, card.cardSuit);
                i++;
            }
        }
        /// <summary>
        /// Subroutine to prompt user to select a card
        /// </summary>
        static void SelectCards(List<Card> hand)
        {
            CardValue outCardValue;
            CardSuit outCardSuit;

        InputCardValue:
            Console.WriteLine("player {0}, select your card. \nEnter Card Value first, followed by Suit. \nEnter p to pass. \nEnter n to confirm. \n", playerIndex + 1);
            string inCardValue = Console.ReadLine();
        Repeat:
            if (inCardValue == "p")
            {
                passCount++;

                selectCards.Clear();
                return;
            }
            if (inCardValue == "n")
            {
                return;
            }
            if (inCardValue == "bark")
            {
                Console.WriteLine("Who would you like to bark at? Please enter 1,2,3 or 4\n");
            Bark:
                if (int.TryParse(Console.ReadLine(), out int playerNum) == true)
                {
                    Console.WriteLine("Arf! Arf! Arf!\n");
                    Console.WriteLine("Player {0} was startled from the barking and fumbled his card!\n", playerNum);
                    Bark(playerNum - 1);
                    goto InputCardValue;
                }
                else
                {
                    Console.WriteLine("Please enter 1,2,3 or 4\n");
                    goto Bark;
                }
            }
            if (inCardValue == "sort")
            {
                Console.WriteLine("Cards in your hand has been re-sorted.\n");
                SortedCards(hand);
                PrintHand(hand);
                goto InputCardValue;
            }
            if (!Enum.TryParse<CardValue>(inCardValue, out outCardValue))
            {
                Console.WriteLine("please re-enter a correct card value. \n");
                goto InputCardValue;
            }
        InputSubgroup:
            string inCardSuit = Console.ReadLine();
            if (!Enum.TryParse<CardSuit>(inCardSuit, out outCardSuit))
            {
                Console.WriteLine("please re-enter a correct card suit. \n");
                goto InputSubgroup;
            }
            Console.WriteLine("Card Value: {0}, Card Suit: {1} \n", outCardValue, outCardSuit);
            int playerHandIndex = hand.FindIndex(p => p.cardValue == outCardValue && p.cardSuit == outCardSuit);
            int selectCardIndex = selectCards.FindIndex(p => p.cardValue == outCardValue && p.cardSuit == outCardSuit);
            if (playerHandIndex == -1)
            {
                Console.WriteLine("...does not exist. Please re-enter another card. \n");
                goto InputCardValue;
            }
            else if (selectCardIndex != -1)
            {
                Console.WriteLine("...has already been selected. Please re-enter another card. \n");
                goto InputCardValue;
            }
            else
            {
                Console.WriteLine("... is selected \n");
                selectCards.Add(hand[playerHandIndex]);
                SortedCards(selectCards);
                Console.WriteLine("Enter n to stop selecting. \n Else type in card value and suit. \n");
                inCardValue = Console.ReadLine();
                if (inCardValue != "n")
                {
                    goto Repeat;
                }
            }
        }
        static void Bark(int intPlayerNum)
        {
            ShuffleDeck(playerHands[intPlayerNum]);
        }
        static void SortedCards(List<Card> listCards)
        {
            listCards.Sort((x, y) => x.cardStrength.CompareTo(y.cardStrength));
        }
        private static Random rng = new Random();
        static FiveCardCombination CardCombinationRanking(List<Card> list)
        {
            bool isFlush = list.GroupBy(c => c.cardSuit).Max(g => g.Count() == 5);
            bool isStraight = !list.Select(person => person.cardValue).ToList().Select((i, j) => i - j).Distinct().Skip(1).Any() == true;// five consecutive ranks. Mind the wheel straight Ace->5!
            if (isStraight == true && isFlush == true)
            {
                return FiveCardCombination.StraightFlush;
            }
            if (list.GroupBy(c => c.cardValue).Max(g => g.Count() == 4))// Return the highest number of element in a group out of the groups in groupby
                return FiveCardCombination.FourOfAKind;
            if (list.GroupBy(c => c.cardValue).Max(g => g.Count()) == 3
                && list.GroupBy(c => c.cardValue).Min(g => g.Count()) == 2)
            {
                return FiveCardCombination.FullHouse;
            }
            if (isFlush == true)
                return FiveCardCombination.Flush;
            if (isStraight == true)
                return FiveCardCombination.Straight;
            return FiveCardCombination.Null;
        }
    }
}
public enum CardSuit
{
    DIAMOND,
    HEART,
    CLUB,
    SPADE,

}
public enum CardValue
{
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace,
    Two,
}
public enum FiveCardCombination
{
    Null,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}