import React, { useState, useRef } from 'react';
import { uploadFile } from '../../services/messageService';

// ============================================
// PROPS INTERFACE
// ============================================
// We're extending the props to accept a new callback for file uploads
interface MessageInputProps {
    onSendMessage: (content: string) => Promise<void>;
    onSendFile?: (file: File) => Promise<void>; // NEW: Optional callback for file uploads
    disabled?: boolean;
    groupId?: string; // We'll need this to construct the upload URL
}

export default function MessageInput({
    onSendMessage,
    onSendFile,
    disabled,
    groupId
}: MessageInputProps) {
    // ============================================
    // STATE MANAGEMENT with useState
    // ============================================

    // STATE 1: Message text input
    // useState returns an array with 2 elements: [currentValue, functionToUpdateValue]
    const [message, setMessage] = useState('');

    // STATE 2: Selected file (can be null if no file is selected)
    // This tracks which file the user has chosen
    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    // STATE 3: Upload progress (0-100)
    // We'll use this later to show a progress bar
    const [uploadProgress, setUploadProgress] = useState(0);

    // STATE 4: Is currently uploading?
    // This prevents multiple simultaneous uploads
    const [isUploading, setIsUploading] = useState(false);

    const [isSending, setIsSending] = useState(false);

    // ============================================
    // REF for File Input
    // ============================================
    // useRef creates a reference to a DOM element
    // Unlike state, changing a ref doesn't trigger a re-render
    // We use it to programmatically click the hidden file input
    const fileInputRef = useRef<HTMLInputElement>(null);

    // ============================================
    // EVENT HANDLERS
    // ============================================

    // Handler for text message submission
    const handleSubmit = async (e: React.FormEvent) => {
        // preventDefault() stops the default form submission behavior
        // (which would reload the page)
        e.preventDefault();

        if (message.trim() && !disabled && !isUploading) {
            setIsSending(true);
            try {
                if (selectedFile) {
                    await handleFileUpload();
                }
                await onSendMessage(message.trim());
                await handleFileUpload(); // Upload file if one is selected
                setMessage(''); // Clear the input after sending
                setIsSending(false);
            } catch (err) {
                // Error is handled by the parent component
            } finally {
                setIsSending(false);
            }
        }
    };

    // Handler for when user clicks the paperclip button
    const handleFileButtonClick = () => {
        // This triggers the hidden file input's click event
        // fileInputRef.current is the actual DOM element
        // The ?. is optional chaining - it only calls click() if current exists
        fileInputRef.current?.click();
    };

    // Handler for when user selects a file
    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        // e.target.files is a FileList (array-like object) of selected files
        // files[0] gets the first file (we only allow single file selection)
        const fileParameter = e.target.files?.[0];

        if (fileParameter) {
            // Update our state with the selected file
            setSelectedFile(fileParameter);
            console.log('File selected:', fileParameter.name, 'Size:', fileParameter.size, 'Type:', fileParameter.type);
        }
    };

    // Handler for uploading the selected file
    const handleFileUpload = async () => {
        // Guard clause: exit early if conditions aren't met
        if (!selectedFile || !groupId || isUploading) return;

        // Set uploading state to true (disables buttons, shows loading state)
        setIsUploading(true);
        setUploadProgress(0);

        try {
            // Convert File to FileParameter format expected by the API
            const fileParameter = {
                data: selectedFile,
                fileName: selectedFile.name
            };

            await uploadFile({
                groupId,
                file: fileParameter
            });

            // Store reference before clearing state
            const uploadedFile = selectedFile;

            // Clear the selected file and reset the input
            setSelectedFile(null);
            setUploadProgress(100);

            // Reset the file input element
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }

            // Optional: If parent provided a callback, call it
            if (onSendFile) {
                await onSendFile(uploadedFile);
            }

        } catch (error) {
            console.error('File upload error:', error);
            alert('Failed to upload file. Please try again.');
        } finally {
            // finally block always runs, even if there's an error
            // Reset the uploading state
            setIsUploading(false);
            setUploadProgress(0);
        }
    };

    // Handler to cancel file selection
    const handleCancelFile = () => {
        setSelectedFile(null);
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
    };

    // ============================================
    // RENDER (JSX)
    // ============================================
    return (
        <form onSubmit={handleSubmit} className="p-4 bg-zinc-900/50 border-t border-zinc-800 backdrop-blur-sm">
            {/* File preview section - only shows when a file is selected */}
            {selectedFile && (
                <div className="max-w-4xl mx-auto mb-3 p-3 bg-zinc-800 rounded-lg border border-zinc-700 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        {/* File icon */}
                        <svg className="w-8 h-8 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>

                        {/* File info */}
                        <div>
                            <p className="text-zinc-100 font-medium">{selectedFile.name}</p>
                            <p className="text-zinc-500 text-sm">
                                {/* Convert bytes to KB/MB */}
                                {(selectedFile.size / 1024).toFixed(2)} KB
                            </p>
                        </div>
                    </div>

                    {/* Action buttons */}
                    <div className="flex gap-2">
                        {/* Cancel button */}
                        <button
                            type="button"
                            onClick={handleCancelFile}
                            disabled={isUploading}
                            className="px-3 py-1 bg-zinc-700 text-zinc-300 rounded-full hover:bg-zinc-600 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                        >
                            X
                        </button>
                    </div>
                </div>
            )}

            {/* Progress bar - only shows during upload */}
            {isUploading && uploadProgress > 0 && (
                <div className="max-w-4xl mx-auto mb-3">
                    <div className="w-full bg-zinc-800 rounded-full h-2">
                        <div
                            className="bg-indigo-600 h-2 rounded-full transition-all duration-300"
                            style={{ width: `${uploadProgress}%` }}
                        />
                    </div>
                </div>
            )}

            {/* Main input area */}
            <div className="flex gap-3 items-center max-w-4xl mx-auto">
                {/* Hidden file input - triggered by the paperclip button */}
                <input
                    ref={fileInputRef}
                    type="file"
                    onChange={handleFileSelect}
                    className="hidden"
                    accept="*/*" // Accept all file types
                />

                {/* Paperclip button - triggers file selection */}
                <button
                    type="button"
                    onClick={handleFileButtonClick}
                    disabled={disabled || isUploading}
                    className="p-3 bg-zinc-800 text-zinc-400 rounded-full hover:bg-zinc-700 hover:text-indigo-400 disabled:opacity-50 disabled:cursor-not-allowed transition-all flex-shrink-0"
                    title="Attach file"
                >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                    </svg>
                </button>

                {/* Text input */}
                <input
                    type="text"
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    placeholder="Type a message..."
                    disabled={disabled || isUploading}
                    className="flex-1 bg-zinc-800 border border-zinc-700 text-zinc-100 rounded-full px-6 py-3 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 placeholder-zinc-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                />

                {/* Send button */}
                <button
                    type="submit"
                    disabled={disabled || !message.trim() || isUploading}
                    className="p-3 bg-indigo-600 text-white rounded-full hover:bg-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg shadow-indigo-500/20 active:scale-95 flex-shrink-0"
                >
                    {disabled || isUploading ? (
                        <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                    ) : (
                        <svg className="w-5 h-5 transform rotate-90" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                        </svg>
                    )}
                </button>
            </div>
        </form>
    );
}
