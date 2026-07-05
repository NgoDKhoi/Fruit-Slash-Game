// Import MediaPipe Hands via CDN (Offscreen/Worker compatible versions)
importScripts('https://cdn.jsdelivr.net/npm/@mediapipe/hands/hands.js');

let hands = null;

// Initialize MediaPipe Hands
function initMediaPipe() {
    hands = new Hands({
        locateFile: (file) => {
            return `https://cdn.jsdelivr.net/npm/@mediapipe/hands/${file}`;
        }
    });

    hands.setOptions({
        maxNumHands: 2,
        modelComplexity: 0, // 0 for max speed/performance
        minDetectionConfidence: 0.5,
        minTrackingConfidence: 0.5
    });

    hands.onResults(onResults);
}

function onResults(results) {
    let outputString = "";
    
    if (results.multiHandLandmarks && results.multiHandLandmarks.length > 0) {
        // For simplicity, we process the first detected hand
        // Format: X,Y,PinchStatus (0 or 1)
        
        let hand = results.multiHandLandmarks[0];
        // Index finger tip is landmark 8
        let indexTip = hand[8];
        // Thumb tip is landmark 4
        let thumbTip = hand[4];
        
        // Calculate distance for pinch detection (basic euclidean)
        let dx = indexTip.x - thumbTip.x;
        let dy = indexTip.y - thumbTip.y;
        let dist = Math.sqrt(dx*dx + dy*dy);
        let isPinching = (dist < 0.05) ? "1" : "0"; // Threshold 0.05
        
        // Output format: IndexX,IndexY,IsPinching
        outputString = `${indexTip.x.toFixed(4)},${indexTip.y.toFixed(4)},${isPinching}`;
    }
    
    // Send back to main thread
    postMessage({ type: 'HAND_DATA', payload: outputString });
}

// Listen for messages from the main thread
onmessage = async function(e) {
    if (e.data.type === 'PROCESS_FRAME') {
        if (!hands) {
            initMediaPipe();
        }
        
        const frameBitmap = e.data.frame;
        if (hands && frameBitmap) {
            await hands.send({image: frameBitmap});
            // ImageBitmap must be closed after use to prevent memory leaks in worker
            frameBitmap.close();
        }
    }
};
