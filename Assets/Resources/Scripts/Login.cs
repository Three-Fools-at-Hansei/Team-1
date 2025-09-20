using UnityEngine;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;


public class Login : MonoBehaviour
{


    
        async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized.");

                // ���� �����̳� �ٸ� ���� ȣ�� ����

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                await SignUpWithUsernamePasswordAsync("qwe123", "Qwe123!!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
   

    async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {

        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(true);
            Debug.Log("�α׾ƿ� �Ϸ�");
        }

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }








    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{ // �Ϲ������� ��ŸƮ���� �ʱ�ȭ ����
        
    //}

    //// Update is called once per frame
    //void Update()
    //{ // �� �����Ӹ��� ����Ǿ�� �ϴ� �ൿ��
        
    //}
}
