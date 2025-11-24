import { useState } from "react";

interface CreateGroupFormProps {
    onSubmit: (data: { name: string; isPublic: boolean }) => void;
    onCancel: () => void;
    isLoading?: boolean;
}

export default function CreateGroupForm({ onSubmit, onCancel, isLoading }: CreateGroupFormProps) {
    const [name, setName] = useState('');
    const [isPublic, setIsPublic] = useState(true);
    const [error, setError] = useState('');

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault(); // Prevents page reload

        if (name.trim().length === 0) {
            setError('Group name is required');
            return;
        }

        if (name.trim().length < 3) {
            setError('Group name must be at least 3 characters');
            return;
        }

        setError('');
        onSubmit({ name: name.trim(), isPublic });
    };


    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <label htmlFor="groupName" className="block text-sm font-medium text-zinc-300 mb-2">
                    Group Name
                </label>
                <input
                    id="groupName"
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Enter group name..."
                    className="w-full px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg 
                             text-zinc-100 placeholder-zinc-500 
                             focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                    disabled={isLoading}
                    autoFocus
                />
                {error && (
                    <p className="mt-1 text-sm text-red-400">{error}</p>
                )}
            </div>

            <div className="flex items-center gap-2">
                <input
                    id="isPublic"
                    type="checkbox"
                    checked={isPublic}
                    onChange={(e) => setIsPublic(e.target.checked)}
                    className="w-4 h-4 text-indigo-600 bg-zinc-800 border-zinc-700 rounded 
                             focus:ring-indigo-500 focus:ring-2"
                    disabled={isLoading}
                />
                <label htmlFor="isPublic" className="text-sm text-zinc-300">
                    Make this group public
                </label>
            </div>

            <div className="flex gap-3 pt-2">
                <button
                    type="button"
                    onClick={onCancel}
                    className="flex-1 px-4 py-2 bg-zinc-800 text-zinc-300 rounded-lg 
                             hover:bg-zinc-700 transition-colors"
                    disabled={isLoading}
                >
                    Cancel
                </button>
                <button
                    type="submit"
                    className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded-lg 
                             hover:bg-indigo-700 transition-colors disabled:opacity-50 
                             disabled:cursor-not-allowed"
                    disabled={isLoading}
                >
                    {isLoading ? 'Creating...' : 'Create Group'}
                </button>
            </div>
        </form>
    );
}