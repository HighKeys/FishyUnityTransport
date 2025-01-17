using System.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FishNet.Example.Transport.UnityTransport.Relay
{
    // Its important to add Unity Services to the project and enable Relay in the UGS Dashboard (https://cloud.unity.com/)
    public class RelayCanvas : MonoBehaviour
    {
        #region Serialized

        [SerializeField] private GameObject ConnectionPanel;
        [SerializeField] private Text JoinCodeText;
        [SerializeField] private Text JoinCodeInputText;
        [SerializeField] Button StartHostButton;
        [SerializeField] Button StartClientOnlyButton;

        #endregion

        private NetworkManager _networkManager;

        void Start()
        {
            _networkManager = InstanceFinder.NetworkManager;
            
            // Setup HostButton Listener, deactivates button after click to prevent multiple clicks
            StartHostButton.onClick.AddListener(() =>
            {
                StartHostButton.interactable = false;
                StartClientOnlyButton.interactable = false;
                _ = StartHostAsync();
            });
            // Setup ClientButton Listener, deactivates button after click to prevent multiple clicks
            StartClientOnlyButton.onClick.AddListener(() =>
            {
                StartHostButton.interactable = false;
                StartClientOnlyButton.interactable = false;
                _ = StartClient(JoinCodeInputText.text);
            });
        }

        private async Task StartHostAsync()
        {
            // Initialize Unity Services
            // It's Important to Initialize Unity Services and Authenticate before using Relay
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // For the example, we will sign in anonymously
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            // get FishyUnityTransport
            var utp = (FishyUnityTransport)_networkManager.TransportManager.Transport;

            // Setup HostAllocation
            Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(4);
            // Get JoinCode from allocation
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);
            utp.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));

            // Start Server Connection
            _networkManager.ServerManager.StartConnection();
            // Start Client Connection
            _networkManager.ClientManager.StartConnection();

            ConnectionPanel.SetActive(false);
            JoinCodeText.text = "Join Code: " + joinCode;
        }

        private async Task StartClient(string joinCode)
        {
            // Initialize Unity Services
            // It's Important to Initialize Unity Services and Authenticate before using Relay
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                // For the example, we will sign in anonymously
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            var utp = (FishyUnityTransport)_networkManager.TransportManager.Transport;
            // get JoinAllocation from JoinCode
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            utp.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            // Start Client Connection
            _networkManager.ClientManager.StartConnection();

            ConnectionPanel.SetActive(false);
        }

    }
}