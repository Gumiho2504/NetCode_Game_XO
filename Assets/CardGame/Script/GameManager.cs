using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;


namespace CardGame
{
    public class GameManager : NetworkBehaviour
    {
        public enum PlayerType
        {
            None, First, Second
        }
        private readonly string[] SUITES = { "H", "D", "C", "S" };//{ "Hearts", "Diamonds", "Clubs", "Spades" };
        private readonly string[] RANKS = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        public Button deal;
        public Transform canvasTransform;
        public Text youText, opponentText;
        [SerializeField] private Sprite[] suitsSprite;
        [SerializeField] private Transform cardPre;
        public NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
        public List<string> youCard;
        public List<string> opponentCard;

        public PlayerType localPlayerType = PlayerType.None;
        public List<string> deck;
        [Header("Transform")]
        public Transform playerOne;
        public Transform playerTwo;


        /*************  ✨ Windsurf Command ⭐  *************/
        /// <summary>
        /// Called when the object is initialized.
        /// Add a listener to the deal button to call the <see cref="DealCartsRpc"/> method when clicked.
        /// </summary>
        /*******  234abbf1-794f-4463-bd0b-2ffcec90d6d0  *******/
        void Start()
        {
            //GenerateCards();
            deal.onClick.AddListener(() => DealCartsRpc(localPlayerType));
        }


        public override void OnNetworkSpawn()
        {
            if (IsLocalPlayer)
            {
                Debug.Log("This is my player object!");
                // Enable input controls
            }
            else
            {
                Debug.Log("This is another player's object.");
                // Disable input controls
            }
            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                localPlayerType = PlayerType.First;
            }
            else
            {
                localPlayerType = PlayerType.Second;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            // playerOne.OnValueChanged += (previousValue, newValue) =>
            // {

            // };
            // playerTwo.OnValueChanged += (previousValue, newValue) =>
            // {

            // };

        }

        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
            {
                if (IsServer)
                {
                    GenerateCards();
                    currentPlayablePlayerType.Value = PlayerType.First;

                }
                isEndGame.OnValueChanged += (previousValue, newValue) =>
                {
                    WinRpc();
                };
                // print($"isServer {IsServer}- isClient {IsClient}-isOwner {IsOwner} - isHost {IsHost}");
            }


        }




        void GenerateCards()
        {
            foreach (string suit in SUITES)
            {
                foreach (string rank in RANKS)
                {
                    deck.Add(rank + suit);


                }
            }
            deck = ShuffleDeck<string>(deck);
            DealCard();

        }




        private void DealCard()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i % 2 == 0)
                {
                    youCard.Add(deck[i]);
                    CloneCard(deck[i], playerOne);
                    CloneCardRpc(deck[i], true);
                }
                else
                {
                    opponentCard.Add(deck[i]);
                    CloneCard(deck[i], playerTwo);
                    CloneCardRpc(deck[i], false);
                }
                deck.RemoveAt(i);
            }
            //   FixedString32Bytes[] youCardArray = youCard.ConvertAll(c => (FixedString32Bytes)c).ToArray();
            //    FixedString32Bytes[] opponentCardArray = opponentCard.ConvertAll(c => (FixedString32Bytes)c).ToArray();
            // ClientCartClientRpc(opponentCardArray, youCardArray);
        }


        [Rpc(SendTo.NotServer)]
        private void CloneCardRpc(string card, bool isYou)
        {
            Transform parent;
            if (isYou)
            {
                opponentCard.Add(card);
                parent = playerTwo;
            }
            else
            {
                youCard.Add(card);
                parent = playerOne;
            }
            CloneCard(card, parent);
        }

        //[Rpc(SendTo.Client)]
        [ClientRpc]
        private void ClientCartClientRpc(FixedString32Bytes[] youCard, FixedString32Bytes[] opponentCard)
        {
            if (localPlayerType == PlayerType.Second)
            {
                this.youCard = youCard.Select(c => c.ToString()).ToList();
                this.opponentCard = opponentCard.Select(c => c.ToString()).ToList();
            }


        }



        [Rpc(SendTo.Server)]
        void DealCartsRpc(PlayerType playerType)
        {
            if (currentPlayablePlayerType.Value != playerType)
            {
                return;
            }
            if (playerType == PlayerType.First)
            {

                print("run on server first");

                youCard.Add(deck[0]);
                print(youCard);
                deck.RemoveAt(0);
                youText.text = deck[0];
                CloneCard(deck[0], playerOne);
                SetOpponentCardClientRpc(deck[0]);

            }
            else if (playerType == PlayerType.Second)
            {
                print("run on server second");
                opponentCard.Add(deck[0]);
                opponentText.text = deck[0];
                CloneCard(deck[0], playerTwo);
                deck.RemoveAt(0);

                TriggerOnDealCartRpc(deck[0]);
                //isEndGame.Value = true;



            }

            switch (playerType)
            {
                default:
                case PlayerType.First:
                    currentPlayablePlayerType.Value = PlayerType.Second;
                    break;
                case PlayerType.Second:
                    currentPlayablePlayerType.Value = PlayerType.First;
                    break;
            }





        }

        NetworkVariable<bool> isEndGame = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        void CloneCard(string card, Transform parent)
        {
            var cardGameObject = Instantiate(cardPre);
            cardGameObject.transform.SetParent(parent, false);
            cardGameObject.transform.localPosition = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), 0);
            Image image = cardGameObject.transform.Find("suite").GetComponent<Image>();
            image.sprite = GetElementSprite(card[1].ToString());
            Text text = cardGameObject.transform.Find("rank").GetComponent<Text>();
            text.text = card[0].ToString();
        }


        [Rpc(SendTo.NotServer)]
        private void SetOpponentCardClientRpc(string card)
        {
            print("runOnClient");
            if (localPlayerType == PlayerType.Second)
            {
                opponentCard.Add(card);
                opponentText.text = card;
                CloneCard(card, playerTwo);
                WinRpc();
            }

        }

        [Rpc(SendTo.ClientsAndHost)]
        void WinRpc()
        {
            if (IsServer) return;
            int youSumValue = ConvertValue(youCard.Sum(c => GetCardValue(c.Contains("10") ? "10" : c[0].ToString())));
            int opponentSumValue = ConvertValue(opponentCard.Sum(c => GetCardValue(c.Contains("10") ? "10" : c[0].ToString())));
            print("youSumValue : " + youSumValue + "opponentSumValue : " + opponentSumValue);
            if (youSumValue > opponentSumValue)
            {
                print("you win");
            }
            else
            {
                print("you lose");
            }
        }



        private int ConvertValue(int value)
        {
            if (value < 10) return value;
            else return value % 10;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void TriggerOnDealCartRpc(string you)
        {

            if (localPlayerType == PlayerType.Second)
            {
                print("run second");
                youCard.Add(you);
                youText.text = you;
                CloneCard(you, playerOne);

            }





        }


        // [ClientRpc(RequireOwnership = false)]
        void SpawnCard(string suit, string rank)
        {
            Transform cardTran = Instantiate(cardPre);
            cardTran.SetParent(canvasTransform, false);
            cardTran.localPosition = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), 0);
            Image image = cardTran.Find("suite").GetComponent<Image>();
            image.sprite = GetElementSprite(suit);

            Text text = cardTran.Find("rank").GetComponent<Text>();
            text.text = rank;

        }




        private List<T> ShuffleDeck<T>(List<T> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                T temp = deck[i];
                int randomIndex = Random.Range(i, deck.Count);
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
            return deck;
        }



        Sprite GetElementSprite(string suit)
        {
            switch (suit)
            {
                case "H": return suitsSprite[0];
                case "D": return suitsSprite[1];
                case "C": return suitsSprite[2];
                case "S": return suitsSprite[3];
                default: return null;
            }
        }

        int GetCardValue(string rank)
        {
            switch (rank)
            {
                case "A": return 1;
                case "K": return 10;
                case "Q": return 10;
                case "J": return 10;
                case "10": return 10;
                case "9": return 9;
                case "8": return 8;
                case "7": return 7;
                case "6": return 6;
                case "5": return 5;
                case "4": return 4;
                case "3": return 3;
                case "2": return 2;
                default: return 0;
            }
        }


    }
}

