import { useEffect, useState } from "react";
import { getGroups, createGroup } from "../../services/groupsService";
import { GroupResponseDto } from "../../generated/api-client";
import Modal from "./Modal";
import CreateGroupForm from "../groups/CreateGroupForm";

interface SidebarProps {
    onSelectGroupId: (groupId: string) => void;
}

export default function Sidebar({ onSelectGroupId }: SidebarProps) {
    const [groups, setGroups] = useState<GroupResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeGroupId, setActiveGroupId] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        const fetchGroups = async () => {
            try {
                setLoading(true);
                const groupsData = await getGroups();
                setGroups(groupsData);
            } catch (err) {
                setError('Failed to fetch groups');
                console.error('Error fetching groups:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchGroups();
    }, []);

    const handleNewGroupClick = () => {
        setShowModal(true);

    };

    const handleCreateGroup = async (data: { name: string; isPublic: boolean }) => {
        try {
            setIsCreating(true);
            const newGroup = await createGroup(data);

            // Add new group to the list
            setGroups(prev => [newGroup, ...prev]);

            // Close modal
            setShowModal(false);

            // Optionally select the new group
            if (newGroup.id) {
                handleGroupClick(newGroup.id);
            }
        } catch (err) {
            console.error('Error creating group:', err);
            alert('Failed to create group. Please try again.');
        } finally {
            setIsCreating(false);
        }
    };

    const handleGroupClick = (groupId: string) => {
        setActiveGroupId(groupId);
        onSelectGroupId(groupId);
    };

    if (loading) {
        return (
            <div className="w-64 h-full bg-zinc-900 border-r border-zinc-800 p-4 flex flex-col gap-4">
                <div className="h-8 bg-zinc-800 rounded animate-pulse w-1/2"></div>
                {[1, 2, 3].map(i => (
                    <div key={i} className="h-12 bg-zinc-800/50 rounded-lg animate-pulse"></div>
                ))}
            </div>
        );
    }

    if (error) {
        return (
            <div className="w-64 h-full bg-zinc-900 border-r border-zinc-800 p-4 text-red-400 text-sm">
                {error}
            </div>
        );
    }

    return (
        <>
            <div className="w-64 h-full bg-zinc-900 border-r border-zinc-800 flex flex-col">
                <div className="p-4 border-b border-zinc-800">
                    <h2 className="text-xs font-semibold text-zinc-500 uppercase tracking-wider mb-4">Groups</h2>
                    <button
                        className="w-full btn-secondary text-sm py-2 flex items-center justify-center gap-2"
                        onClick={handleNewGroupClick}
                    >
                        <span className="text-lg leading-none">+</span> New Group
                    </button>
                </div>

                <div className="flex-1 overflow-y-auto p-2 space-y-1">
                    {groups.map((group: GroupResponseDto) => (
                        <Group
                            key={group.id}
                            name={group.name}
                            isActive={activeGroupId === group.id}
                            onClick={() => group.id && handleGroupClick(group.id)}
                        />
                    ))}
                </div>
            </div>

            <Modal
                isOpen={showModal}
                onClose={() => setShowModal(false)}
                title="Create New Group"
            >
                <CreateGroupForm
                    onSubmit={handleCreateGroup}
                    onCancel={() => setShowModal(false)}
                    isLoading={isCreating}
                />
            </Modal>
        </>
    );
}

function Group({ name, isActive, onClick }: { name: string | undefined, isActive: boolean, onClick: () => void }) {
    return (
        <div
            onClick={onClick}
            className={`
                group flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-all duration-200
                ${isActive
                    ? 'bg-indigo-600/10 text-indigo-400'
                    : 'text-zinc-400 hover:bg-zinc-800 hover:text-zinc-100'
                }
            `}
        >
            <div className={`
                w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold transition-colors
                ${isActive
                    ? 'bg-indigo-600 text-white'
                    : 'bg-zinc-800 text-zinc-500 group-hover:bg-zinc-700 group-hover:text-zinc-300'
                }
            `}>
                {name?.charAt(0).toUpperCase() || '?'}
            </div>
            <span className="font-medium truncate">{name || 'Unnamed Group'}</span>
        </div>
    );
}