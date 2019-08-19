/*
 * MessageQueue.cs
 * 
 * Class for thread safe messsage queue intended to be
 * used for interthread communication
 * 
 * This class is a part of Project VR SAD's tool kit
 *       
 *
 * AUTHOR     : Alphin Edgar D'cruz
 * START DATE : April 2019
 *
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace VRSAD.ToolKit
{

    public class MessageQueue
    {

        private Mutex        pushLock;
        private Mutex        pullLock;
        private SpinWait     spin;
        private List<string> msgQueue;
        

        /*
         * Default MessageQueue constructor
         * 
         * Initializes class objects
         */
        public MessageQueue()
        {
            pushLock = new Mutex();
            pullLock = new Mutex();
            spin     = new SpinWait();
            msgQueue = new List<string>();
        }

        /*
         * Function WaitAndPullTop
         * 
         * IN   : -
         * OUT  : string
         * DESC : This function will pull a message from the top
         *        of the queue, if no message exists it waits for
         *        a message to be pushed. Is thread safe.
         */
        public string WaitAndPullTop()
        {
            pullLock.WaitOne();

            while (msgQueue.Count == 0)
                spin.SpinOnce();

            string tosend = msgQueue[0];
            msgQueue.RemoveAt(0);

            pullLock.ReleaseMutex();

            return tosend;
        }

        /*
         * Function WaitAndPullTop
         * 
         * IN   : ms: int
         * OUT  : string
         * DESC : This function will pull a message from the top
         *        of the queue, if no message exists it waits for
         *        a message to be pushed for ms milliseconds then
         *        returns null if no message has been pushed. Is
         *        thead safe.
         */
        public string WaitAndPullTop(int ms)
        {

            if (!pullLock.WaitOne(ms))
                return null;

            ms = ms / 250;
            int i = 0;
            while (msgQueue.Count == 0 && i < ms)
            {
                System.Threading.Thread.Sleep(250);
                i++;
            }

            if (msgQueue.Count == 0)
                return null;

            string tosend = msgQueue[0];
            msgQueue.RemoveAt(0);

            pullLock.ReleaseMutex();


            return tosend;
        }

        /*
         * Function WaitInBackgroundAndCallBack
         * 
         * IN   : callBack: Action<string>
         * OUT  : void
         * DESC : This function returns after creating a thread
         *        that will pull a message from the top of the 
         *        queue, if no message exists it waits for a message 
         *        to be pushed. The pulled message is passed as an
         *        argument to the callback function. Is thread safe
         */
        public void WaitInBackgroundAndCallBack(Action<string> callBack)
        {
            System.Threading.Thread pullMessageJob = new System.Threading.Thread(WaitAndCallBack);
            pullMessageJob.Start(callBack);
        }

        /*
         * Function waitAndCallBack
         * 
         * IN   : callBack: Action<string>
         * OUT  : void
         * DESC : This function will pull a message from the top of the 
         *        queue, if no message exists it waits for a message 
         *        to be pushed. The pulled message is passed as an
         *        argument to the callback function. Is thread safe
         */
        public void WaitAndCallBack (object data)
        {
            pullLock.WaitOne();

            while (msgQueue.Count == 0)
                spin.SpinOnce();

            string tosend = msgQueue[0];
            msgQueue.RemoveAt(0);

            pullLock.ReleaseMutex();


            Action<string> callBack = data as Action<string>;
            callBack(tosend);
        }

        /*
         * Function pushMsg
         * 
         * IN   : msg: string
         * OUT  : -
         * DESC : This function will push a message to the end of
         *        the queue. Is thread safe.
         */
        public void PushMsg(string msg)
        {
            pushLock.WaitOne();

            msgQueue.Add(msg);

            pushLock.ReleaseMutex();
        }
    }
}
