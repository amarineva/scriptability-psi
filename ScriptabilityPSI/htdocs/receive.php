<?php

/* Function to form acknowledgement message */
function sendResponse($message_id)
{
	$response = '<?xml version="1.0" encoding="utf-8"?>';
	$response .= '<XML><Message_Header><Message_ID>'.$message_id.'</Message_ID>';
	$response .= '<Message_Type>ACK</Message_Type></Message_Header></XML>';
	return $response;
}

/* Function to generate a temporary log file */
function writeLog($data,$filename,$bEOL)
{
	$log_fp = fopen($filename,"a");
	if($log_fp)
	{
		while (!flock($log_fp, LOCK_EX)) {
			usleep(10);
		}
		if($bEOL == true)
			fwrite($log_fp,date("Y-m-d H:i:s")."\t".$data.PHP_EOL);
		else
			fwrite($log_fp,date("Y-m-d H:i:s")."\t".$data);
	
		flock($log_fp, LOCK_UN);
		fclose($log_fp);
	}
	
}
/* Function to remove the special characters from xml node value */
function remove_special_characters($text)
{
    return  preg_replace(array('/(\r\n\r\n|\r\r|\n\n)(\s+)?/', '/\r\n|\r|\n/','/[ \t]/'),
            array(''), $text);
}
/* Logging the request receival */
writeLog("The listener received the wake up signal.","log.txt",true);
/* Processing the posted data from the pioneer Rx software */
$post_data = file_get_contents("php://input");

if( strlen($post_data) <= 0 )
	writeLog("Empty data received.","log.txt",true);
else	
	/*Logging the posted data */
	writeLog($post_data,"log.txt",true);

/* Converting into simple xml object */
$xml_object = simplexml_load_string($post_data);

/* Converting simple xml object into DOM object for further parsing */
$dom = new DOMDocument();
$dom->loadXML($xml_object->asXML());
$dom->formatOutput = true;

/* Parsing the message id */
$msg_id_node = $dom->getElementsByTagName('MessageID');
$msg_id= $msg_id_node->item(0)->nodeValue;

/* Parsing message sent time */
$msg_sent_time_node = $dom->getElementsByTagName('SentOnUTC');
$msg_sent_time = $msg_sent_time_node ->item(0)->nodeValue;

/* Reading the path to save the label files */
$label_path_file = fopen("Label_Path.txt", "r") or die("Unable to locate label path file!!!");
$label_path = fread($label_path_file,filesize("Label_Path.txt"))."\\tempLabel";
fclose($label_path_file);

if (!is_dir($label_path)) {
    // Create the directory with permissions of 0700 to make it hidden
    mkdir($label_path);
	exec('attrib +h '.$label_path);
}

/* Parsing Rx number */
$rx_number_node = $dom->getElementsByTagName('Rx')->item(0)->getElementsByTagName('RxNumber');
$rx_number = $rx_number_node->item(0)->nodeValue;
if($rx_number == "")
{
	return(0);
}

/* Forming the absolute path for saving label files */
$label_file_name = $label_path.'/'.$rx_number_node->item(0)->nodeValue.".lbl";

/* Forming patient name by parsing first and last name of patient */
$patient_first_name_node = $dom->getElementsByTagName('Patient')->item(0)->getElementsByTagName('Name')->item(0)->getElementsByTagName('FirstName');
$patient_name = remove_special_characters($patient_first_name_node->item(0)->nodeValue);

$patient_last_name_node = $dom->getElementsByTagName('Patient')->item(0)->getElementsByTagName('Name')->item(0)->getElementsByTagName('LastName');
$patient_name = $patient_name." ". remove_special_characters($patient_last_name_node->item(0)->nodeValue);
$label_content = $patient_name.";";

/* Parsing Medication name */
$medication_node = $dom->getElementsByTagName('DrugName');
$medication = $medication_node->item(0)->nodeValue;
$label_content = $label_content.$medication.";";

/* Parsing instructions */
$instruction_node = $dom->getElementsByTagName('DirectionsTranslatedEnglish');
$instruction = $instruction_node->item(0)->nodeValue;
$label_content = $label_content.$instruction .";";

/* Parsing quantity */ 
/* if MedicationDispensed->Quantity is empty then read MedicationPrescribed->Quantity ===> #296*/
$quantity_node = $dom->getElementsByTagName('MedicationDispensed')->item(0)->getElementsByTagName('Quantity');
$quantity = remove_special_characters($quantity_node->item(0)->nodeValue);
if(empty($quantity))
{
	$quantity_node = $dom->getElementsByTagName('MedicationPrescribed')->item(0)->getElementsByTagName('Quantity');
	$quantity = remove_special_characters($quantity_node->item(0)->nodeValue);
}
$label_content = $label_content.$quantity.";";

/* Parsing refills remaining */
$refill_remain_node = $dom->getElementsByTagName('RefillsRemaining');
$refill_remain = remove_special_characters($refill_remain_node->item(0)->nodeValue);
$label_content = $label_content.$refill_remain.";";

/* Parsing Prescriber name */
$prescriber_first_name_node = $dom->getElementsByTagName('Prescribers')->item(0)->getElementsByTagName('Prescriber')->item(0)->getElementsByTagName('Name')->item(0)->getElementsByTagName('FirstName');
$prescriber = remove_special_characters($prescriber_first_name_node->item(0)->nodeValue);

$prescriber_last_name_node = $dom->getElementsByTagName('Prescribers')->item(0)->getElementsByTagName('Prescriber')->item(0)->getElementsByTagName('Name')->item(0)->getElementsByTagName('LastName');
$prescriber = $prescriber." ". remove_special_characters($prescriber_last_name_node->item(0)->nodeValue);
$label_content = $label_content.$prescriber.";";

/* Parsing pharmacy name */
$pharmacy_name_node = $dom->getElementsByTagName('PharmacyName');
$pharmacy = $pharmacy_name_node->item(0)->nodeValue;
$label_content = $label_content.$pharmacy.";";

/* Parsing phone number */
$phone_areacode_node = $dom->getElementsByTagName('Pharmacy')->item(0)->getElementsByTagName('PhoneNumbers')->item(0)->getElementsByTagName('PhoneNumber')->item(0)->getElementsByTagName('AreaCode');
$phone = remove_special_characters($phone_areacode_node->item(0)->nodeValue);
$phone = "(".$phone.")";

$phone_number_node = $dom->getElementsByTagName('Pharmacy')->item(0)->getElementsByTagName('PhoneNumbers')->item(0)->getElementsByTagName('PhoneNumber')->item(0)->getElementsByTagName('Number');
$phone = $phone.remove_special_characters(substr_replace($phone_number_node->item(0)->nodeValue, '-', 3, 0));
$label_content = $label_content.$phone.";";

/* Parsing rx number */
$rx_number = remove_special_characters($rx_number);
$label_content = $label_content.$rx_number.";";

/* Parsing NDC number */
$ndc_node = $dom->getElementsByTagName('MedicationDispensed')->item(0)->getElementsByTagName('NDC');
$ndc = remove_special_characters($ndc_node->item(0)->nodeValue);
$label_content = $label_content.$ndc.";";

/* Adding empty for warnings and other info field */
$label_content = $label_content.";".";".";".";".";".";";

/* Parsing Fill Date */
$fill_date_node = $dom->getElementsByTagName('DateFilledUTC');
$fill_date = strtok($fill_date_node->item(0)->nodeValue, 'T');
$date_obj = new DateTime($fill_date);
$fill_date = remove_special_characters($date_obj->format('m/d/Y'));
$label_content = $label_content.$fill_date.";";

/* Parsing Use By Date */
$expiry_date_node = $dom->getElementsByTagName('LotExpirationDate');
$expiry_date = strtok($expiry_date_node->item(0)->nodeValue, 'T');
$date_obj = new DateTime($expiry_date);
$expiry_date  = remove_special_characters($date_obj->format('m/d/Y'));
$label_content = $label_content.$expiry_date.";";

/* Adding empty field between Use By Date and Refillable Until Date */
$label_content .= ";";

/* Parsing Refillable Until Date */
$expiry_date_node = $dom->getElementsByTagName('ExpirationDateUTC');
$expiry_date = strtok($expiry_date_node->item(0)->nodeValue, 'T');
$date_obj = new DateTime($expiry_date);
$expiry_date  = remove_special_characters($date_obj->format('m/d/Y'));
$label_content = $label_content.$expiry_date.";";

/* Adding a newline after the mandtory field information */
$label_content = $label_content.PHP_EOL;

/* Adding font attributes settings */
$label_content = $label_content."FONT_FACE = Arial Black";
$label_content = $label_content.PHP_EOL;
$label_content = $label_content."FONT_SIZE = 18";
$label_content = $label_content.PHP_EOL;
$label_content = $label_content."NEGATED_IMAGE = FALSE";

/* Get the Previous created file name */
$info = pathinfo($label_file_name);
$label_file_name_lbxa = $info['dirname']."/".$info['filename'].'.'."lbxa";
$label_file_name_lbxj = $info['dirname']."/".$info['filename'].'.'."lbxj";
//echo date_default_timezone_get();
//echo date("Ymd h:i:sa");
/* Verify the file exists in the same directory or not */
if (file_exists($label_file_name_lbxa) || file_exists($label_file_name_lbxj) ) {
    $file_name = $info['dirname']."/".$info['filename']."_".date("Ymd_His").'.'."lbl";
} else {
    $file_name = $info['dirname']."/".$info['filename'].'.'."lbl";
}


/* Open the label file in the write mode for copying the label content */
$label_file_handle = fopen($file_name, "w") or die("Unable to open label file!!!");
fwrite($label_file_handle ,$label_content);
fclose($label_file_handle);

$label_file = '"'.$file_name.'"';
#echo $label_file;
//if($answer = shell_exec("Encryptor.exe $label_file"))
//{
//	writeLog("Fail to Encrypt the file.","log.txt",false);
//	echo "Fail to Encrypt the file";
	#echo $answer;
//	exit;
//}
shell_exec("Encryptor.exe");
writeLog("The data has been encrypted successfully.","log.txt",false);
/* Encrypt and append the temporary logs to the main log file */
//if($answer = shell_exec("Encryptor.exe -a log.txt log.txtx"))
//
//	echo "Fail to Encrypt the log file";
	#echo $answer;
//	exit;
//}
/* Sending the acknowledgement packet */
header("Content-type: text/xml; charset=utf-8");
echo sendResponse($msg_id);

?>
