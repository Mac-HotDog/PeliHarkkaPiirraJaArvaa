
import React, { useState } from 'react';
import './Chat.css';

const SendMessageForm = ({ sendMessage }) => {
    const [message, setMessage] = useState('');

    return <form
        onSubmit={e => {
            e.preventDefault();
            sendMessage(message);
            setMessage('');
        }}>

        <input className="inputti" maxLength="25" onChange={e => setMessage(e.target.value)} value={message} type="text" placeholder="message..." />
            <button className="nappi" type="submit" disabled={!message}>Send</button>
        
    </form>
}

export default SendMessageForm;