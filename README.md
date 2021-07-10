# Co-WIN: _Vaccine Finder_
<ins>Vaccine Finder</ins>: Based on Pin-Codes, Age Limit Criteria, Dose, Vaccine and other User preferences.
Sends notification on Mail and Telegram, if slots are available.
Book Slots and Send notification on Mail and Telegram, if Booked successfully.

<ins>_Platforms Supported_</ins>: 
- [x] Windows
- [x] macOS
- [x] Linux

<ins>_Installation instructions_</ins>:
1. Unzip folder.
2. Run VaccineFinder.exe
3. Fill in the basic details.
_Note_: Details once filled, will be retained to use for next time. It will be picked automatically. You can also check appsettings.json for more details.


<ins>***User Inputs***</ins>:

_Mandatory_:
1. Phone number
2. Pin-Codes : Search is based on Priority order defined (first come basis). Format - Comma separated (eg. 100001, 100002)
3. Beneficiary IDs: IDs found in Co-Win app against individual user. Don't worry, we will fetch everthing for you. Format - Comma separated (eg. 123456789, 789101134).
4. Dose (eg. 1 or 2)
5. Vaccine (eg. ANY, COVISHIELD, COVAXIN, SPUTNIK V)
6. Email Ids: Email ids where mail will be sent in case of slots availability. Format - Comma separated (eg. abc@gmail.com, def@gmail.com)
7. Minimum Age Limit: 18 or 45
8. Slot Preference: (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)
9. From Date: Date from which vaccination slots will be searched (for 7 days). Format - "dd-MM-yyyy" (eg. 31-03-2021)
10. Retry Frequency (in Seconds): Frequency at which retry call should happen (in case of no slots found), (eg. 10). Value less than 3 Seconds is not recommended due to rate limiting imposed by the Government.
11. AutoPickCenter - If True ("1"), it auto picks center with the combination of 'Pin-code priority and the center with most available slots'. If false, it asks user to input preferred center.
12. IncludePaidService - If True ("1"), it will also include centers with as 'Paid' Service (Fee Type). If false, it will only pick centers with 'Free' service.
13. Telegram Chat ID: Chat ID with @Covid19VaccineSlotFinderBot, will not send Notification if this field is left Empty in appsettings.json. You can Get your Chat ID by Pinging @Covid19VaccineSlotFinderBot, and then we can update this on the fly based on UserName.

_Optional_:
1. First Name - will be used in E-Mail.
2. Last Name - will be used in E-Mail.
	
_In **appsettings.json**_: (Highly Recommended not to update these settings)
1. VerifyBeneficiaries - To verify beneficiaries, before slot booking (highly recommended: True)
2. SendEmail - True or False to enable/disable Mail.
3. SendNotification - True or False to enable/disable Telegram Notifications.
4. AutomaticCloseProgramWaitTime (in Seconds)- At the point of termination, program will wait for seconds before closing Automatically.
5. TryToBookOtherSlots - True or False. Try to Book Slots other than Slot Preference specified in UserPreference if enabled ("1"), By default disabled ("0") as we think availability is for the entire day, it is not segregated in individual slots

### <ins>***Process***</ins>:

1. Check Latest Version of the Application.
2. Confirm inputs from user.
3. Generate OTP.
4. Verify OTP.
5. Verify Beneficiaries. Verify Vaccine and Dose specified, Update valid Vaccine and Dose in settings automatically.
6. Check Available slots.
   - Case a) Vaccine slots available: Program will Display Available Centres, make a beep sound to notify, sends Notifications over E-Mail (on defined Email Ids), and Telegram. Proceed to Booking (next step).			
   - Case b) Vaccine slots not available: Program will retry at user defined Retry Frequency. Automatically re-generate OTP, if session is expired, or IP is blocked.
7. Start Booking:
   - _AutoBookCenter_:
	 - case a) If AutoBookCenter is False ("0"), get preferred center input (number) from user, and only try to book slots of specific center. 
	 - case b) If AutoBookCenter is True ("1"), it will sort available centers based on Pin-Code preference, then sort by availability. Then tries to book slot, it will hit all the centers until slot is successfully booked.
   - Try to Book Slot, with Slot preference. If booking fails, it tries to book other slots (depending on _TryToBookOtherSlots_ setting in appsettings.json) whichever is available.
   - Sends Confirmation Notification over E-Mail (on defined Email Ids), and Telegram.
8. Automatically close in 30 seconds (depends on _AutomaticCloseProgramWaitTime_ setting in appsettings.json).

[Click Here To Watch The Demo](https://www.youtube.com/watch?v=z_5E703sMKY)

### <ins>***Application Features***</ins>

Following are some of the main features of the App:
- [x] Extremely User Friendly Settings: All the preferences can be Customized. User can Control Everthing.
- [x] One Time Setup: User Preferences will be saved in Settings, so need to set ever again. Just input the OTP and you are good to go anytime.
- [x] Intelligent App: The App can VALIDATE beneficiaries, IDENTIFY invalid inputs (like beneficiaries combination, Dose, Vaccine etc.), Automatically UPDATES valid values and SAVE Settings.
- [x] BEEP Notifier for user, in case of any important event - OTP, Availability, Booking, Session Timeout, IP Blocked.
- [x] Auto-Regenerate OTP in case of Session Timeout, IP Blocked.
- [x] Integration of Notification Engine with Telegram Bot
- [x] Verbose Mode so that user can check each and every step as what is going on behind the scenes when slots are being fetched & processed
- [x] Zero Setup Application, Just Download and Run! Easy to Use!
- [x] No specialized Software required for modification of Config file, it could be done inside the App.


> <ins>***Note***</ins>: _This is a Personal Project, We do NOT endorse this for Public use. Developed for Educational Purpose only; USE AT YOUR OWN RISK. DEVELOPER SHOULD NOT BE DEEMED RESPONSIBLE FOR ANY LEGAL CONCERNS. DO NOT SHARE THIS. ANY USE OF THE SCRIPT FOR MONETARY, UNETHICAL, OR ILLEGAL PURPOSES IS NOT PERMITTED. - IN CASE, YOU ARE FOUND TO DO THE SAME, YOU SHALL BE SOLELY LIABLE FOR THE CONSEQUENCES. Developer reserves the right to take any legal action as enumerated under the law against any unethical or illegal use of the same._