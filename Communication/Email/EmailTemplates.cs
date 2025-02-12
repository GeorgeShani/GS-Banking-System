using Banking_System.Models;
using BankAccount = Banking_System.Models.Account;

namespace Banking_System.Communication.Email;

public static class EmailTemplates
{
    public static string WelcomeEmail(Client client)
    {
      return $"""
              <!DOCTYPE html>
              <html lang="en">
              <head>
                  <meta charset="UTF-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1.0">
                  <title>Welcome to GS Bank</title>
              </head>
              <body style="margin:0; padding:0; font-family:Arial, sans-serif; background-color:#f9f9f9; color:#333;">
                <div style="width:100%; max-width:600px; margin:20px auto; background:#ffffff; padding:20px; border:1px solid #ddd; border-radius:5px;">
                  <div style="text-align:center; padding-bottom:20px;">
                    <h1 style="margin:0; color:#4CAF50;">Welcome to GS Bank</h1>
                  </div>
                  <div style="line-height:1.6;">
                    <p>Dear {client.FirstName},</p>
                    <p>Thank you for joining GS Bank. We're excited to have you on board.</p>
                    <p>If you have any questions, feel free to contact our support team at <a href="mailto:gsbank308@gmail.com" style="color:#4CAF50;">gsbank308@gmail.com</a>.</p>
                    <p>Best regards,</p>
                    <p>The GS Bank Team</p>
                  </div>
                  <div style="text-align:center; font-size:12px; color:#777; margin-top:20px;">
                    &copy; 2025 GS Bank. All rights reserved.
                  </div>
                </div>
              </body>
              </html>
              """;
    }

    public static string TransactionAlertEmail(Client client, BankAccount account, Transaction transaction)
    {
      return $"""
             <!DOCTYPE html>
             <html lang="en">
             <head>
                 <meta charset="UTF-8">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
                 <title>Transaction Alert</title>
             </head>
             <body style="margin:0; padding:0; font-family:Arial, sans-serif; background-color:#f9f9f9; color:#333;">
               <div style="width:100%; max-width:600px; margin:20px auto; background:#ffffff; padding:20px; border:1px solid #ddd; border-radius:5px;">
                 <div style="text-align:center; color:#FF5722; padding-bottom:20px;">
                   <h1 style="margin:0;">Transaction Alert</h1>
                 </div>
                 <div style="line-height:1.6;">
                   <p>Dear {client.FirstName},</p>
                   <p>A transaction has been made from your account ({account.AccountNumber}):</p>
                   <ul>
                     <li><strong>Amount:</strong> {transaction.Amount} {account.Currency}</li>
                     <li><strong>Type:</strong> {transaction.TransactionType.ToString()}</li>
                     <li><strong>Date:</strong> {transaction.TransactionDate}</li>
                     <li><strong>Status:</strong> {transaction.Status.ToString()}</li>
                     <li><strong>Description:</strong> {transaction.Description}</li>
                   </ul>
                   <p>If you did not authorize this transaction, please contact us immediately at <a href="mailto:gsbank308@gmail.com" style="color:#FF5722;">gsbank308@gmail.com</a>.</p>
                   <p>Thank you for banking with GS Bank.</p>
                 </div>
                 <div style="text-align:center; font-size:12px; color:#777; margin-top:20px;">
                   &copy; {DateTime.Now.Year} GS Bank. All rights reserved.
                 </div>
               </div>
             </body>
             </html>
             """;
    }

    public static string OtpVerificationEmail(Client client, string oneTimePassword)
    {
      return $"""
             <!DOCTYPE html>
             <html lang="en">
             <head>
                 <meta charset="UTF-8">
                 <meta name="viewport" content="width=device-width, initial-scale=1.0">
                 <title>Password Reset with OTP</title>
             </head>
             <body style="margin:0; padding:0; font-family:Arial, sans-serif; background-color:#f9f9f9; color:#333;">
               <div style="width:100%; max-width:600px; margin:20px auto; background:#ffffff; padding:20px; border:1px solid #ddd; border-radius:8px;">
                 <div style="text-align:center; color:#3F51B5; padding-bottom:20px;">
                   <h1 style="margin:0;">Your One-Time Password</h1>
                 </div>
                 <div style="line-height:1.6;">
                   <p>Dear {client.FirstName},</p>
                   <p>To reset your password or verify your account, use the following One-Time Password (OTP):</p>
                   <p style="font-size:24px; font-weight:bold; text-align:center; color:#3F51B5; margin:20px 0;">{oneTimePassword}</p>
                   <p>This OTP is valid for the next 10 minutes. If you did not request this, please ignore this email or contact our support team at <a href="mailto:gsbank308@gmail.com" style="color:#3F51B5;">gsbank308@gmail.com</a>.</p>
                   <p>Thank you,</p>
                   <p>The GS Bank Team</p>
                 </div>
                 <div style="text-align:center; font-size:12px; color:#777; margin-top:20px;">
                   &copy; {DateTime.Now.Year} GS Bank. All rights reserved.
                 </div>
               </div>
             </body>
             </html>
             """;
    }
}