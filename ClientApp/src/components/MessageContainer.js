import React, { useEffect, useRef } from 'react';
import './Chat.css';


const MessageContainer = ({ messages, sana, gameWinner, userRole }) => {
    const messageRef = useRef();

    useEffect(() => {
        if (messageRef && messageRef.current) {
            const { scrollHeight, clientHeight } = messageRef.current;
            messageRef.current.scrollTo({ left: 0, top: scrollHeight - clientHeight, behavior: 'smooth' });
        }
    },
        [messages]);

    function ValkoinenPohja(props) {
        const viesti2 = props.viesti2;
        
        return <div className='message-color-white'>{viesti2}</div>;
    }
    function VihreaPohjaPiirtaja(props) {
        return <div className='message-color-green'>You win the round, correct word was {sana}.</div>;
    }

    function VihreaPohjaArvaaja(props) {
        const pelaajaVoittaja = props.pelaajaVoittaja;
        return <div className='message-color-green'>{pelaajaVoittaja} win the round, correct word was {sana}!</div>;
    }

    function PunainenPohja(props) {
        const pelaajaVoittaja = props.pelaajaVoittaja;
        return <div className='message-color-red'>{pelaajaVoittaja} is the winner!</div>;
    }
    function VariValitsin(props) {
        const voitto = props.voitto;
        const viesti = props.viesti;
        const pelaaja = props.pelaaja;

        if (voitto && gameWinner) {
            if (viesti != sana || !sana) {
                return <ValkoinenPohja viesti2={viesti} />;
            }
            return <PunainenPohja pelaajaVoittaja={gameWinner} />;
        }

        else if (voitto && userRole) {
            if (viesti != sana || !sana) {
                return <ValkoinenPohja viesti2={viesti} />;
            }
            return <VihreaPohjaPiirtaja pelaajaVoittaja={pelaaja} />;
        }

        else if (voitto && !userRole) {
            if (viesti != sana || !sana) {
                return <ValkoinenPohja viesti2={viesti} />;
            }
            return <VihreaPohjaArvaaja pelaajaVoittaja={pelaaja} />;
        }
        return <ValkoinenPohja viesti2={viesti} />;
    }

    return <div ref={messageRef} className='message-container' >
        {messages.map((m, index) =>
           
            <div key={index} className='user-message'>

                <VariValitsin voitto={m.win} viesti={m.message} pelaaja={m.user} />                
                
            </div>
        )}
    </div>
}

export default MessageContainer;