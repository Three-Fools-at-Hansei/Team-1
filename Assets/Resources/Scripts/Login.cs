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

                // 이후 인증이나 다른 서비스 호출 가능

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
            Debug.Log("로그아웃 완료");
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
    //{ // 일반적으로 스타트에는 초기화 문법
        
    //}

    //// Update is called once per frame
    //void Update()
    //{ // 매 프레임마다 실행되어야 하는 행동들
        
    //}
}
