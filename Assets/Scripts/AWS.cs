using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using UnityEngine.UI;
using Amazon.DynamoDBv2.Model;
using Amazon;
using Amazon.Runtime.Internal;

public class AWS : MonoBehaviour
{
    private AmazonDynamoDBClient client;
    private DynamoDBContext context;
    // Start is called before the first frame update

    //Get drop down info
    public InputField inputUser;
    public Text txtUser;
    public Text txtParticipation;
    public Text txtStreamCount;
    public Text txtStatus;

    private bool initialerror = false;
    private bool finishSearch = false;
    //public Text txtStatus;
#if UNITY_ANDROID
    public void UsedOnlyForAOTCodeGeneration()
    {
        //Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
        //IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
        //Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html

        AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
        int valueString = jo.Get<int>("what");
    }
#endif
    void Start()
    {
        txtUser.enabled = false;
        txtParticipation.enabled = false;
        txtStreamCount.enabled = false;
        
        txtStatus.enabled = false;
        UnityInitializer.AttachToGameObject(gameObject);  // a error resolution 
        gameObject.AddComponent<UnityMainThreadDispatcher>();  // If you don't do it once, a namespace error will appear. 

        AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();



        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        // Initialize the Amazon Cognito credentials provider
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "us-east-1:714fd823-444e-44ed-ac60-1f601ad14f24", // Identity pool ID
            RegionEndpoint.USEast1 // Region
        );

        client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        context = new DynamoDBContext(client);
        DescribeTable();

    }

    void Update()
    {
       
        if (initialerror == true){
            if (finishSearch == false)
            {
             
                   
                    StartCoroutine(WaitSearch());
             
            }
        }
    }

    IEnumerator WaitSearch()
    {
        txtStatus.enabled = true;
        txtStatus.text = "searching...";
        yield return new WaitForSeconds(1);
        //If text cannot be filled out, switch txtStatus to user not found
        if(txtUser.enabled == false)
        {
            txtStatus.text = "user not found";
         
        }
        else
        {
            txtStatus.enabled = false;
        }
        finishSearch = true;
    }
    public void DescribeTable()
    {
        var request = new DescribeTableRequest
        {
            TableName = @"OverlayStatistics"
        };

        //var response = client.DescribeTable(new DescribeTableRequest { TableName = "PlanB" });

        //Debug.Log(response.Table.ItemCount);
        client.DescribeTableAsync(request, (result) =>
        {
            if (result.Exception != null)
            {

                Debug.Log("DESCRIBE ERROR: " + result.Exception);
                return;
            }
            var response = result.Response;
            TableDescription description = response.Table;

            Debug.Log(description.TableName.ToString());
            //GetTotalCompleted();
        }, null);



    }

    public void SearchPlayer()
    {

        RetrieveUserInfo(inputUser.text);
        initialerror = true;
        finishSearch = false;
    }

    public void RetrieveUserInfo(string username)
    {
        Debug.Log("LOOKING users...");
        //Get user from drop down
        //txtStatus.enabled = false;
        txtUser.enabled = false;
        txtParticipation.enabled = false;
        txtStreamCount.enabled = false;
        //txtStatus.enabled = false;
        //search for username
        //bool foundUser = false;
        UserObject userRetrieved = null;
        context.LoadAsync<UserObject>(username.ToLower(), (result) =>
        {
            //print("RESULT: "+ result.Result.ToString());
            if (result.Exception == null)
            {

                userRetrieved = result.Result as UserObject;
                // Update few properties.
                //userRetrieved.user = username;
                // Replace existing authors list with this
                List<string> listDates = userRetrieved.dates;

                userRetrieved.dates = listDates;
                txtUser.enabled = true;
                txtParticipation.enabled = true;
                txtStreamCount.enabled = true;
                txtUser.text = "Quacker: " + username.ToLower();
                txtStreamCount.text = "Stream Participation: " + listDates.Count.ToString();
                string dateString = listDates[listDates.Count - 1];
                System.DateTime dateFormat = System.DateTime.Parse(dateString);


                txtParticipation.text = "Last Participated: " + dateFormat.ToString("yyy-MM-dd");
               

            }

    

        });



    }

    [DynamoDBTable("OverlayStatistics")]
    public class UserObject
    {
        [DynamoDBHashKey]
        public string user { get; set; }
        //[DynamoDBProperty]
        //public string user { get; set; }
        [DynamoDBProperty]
        public List<string> dates { get; set; }

    }
}
