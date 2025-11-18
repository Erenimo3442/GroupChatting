import { useEffect, useState } from "react";
import { getGroups, createGroup } from "../../services/groupsService";
import { GroupResponseDto } from "../../generated/api-client";

interface SidebarProps {
    onSelectGroupId: (groupId: string) => void;
}

export default function Sidebar({ onSelectGroupId }: SidebarProps) {
    const [groups, setGroups] = useState<GroupResponseDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

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

    if (loading) {
        return <div className="col-span-6 row-span-9 md:col-span-2 lg:col-span-1 border-r border-gray-300">Loading...</div>;
    }

    if (error) {
        return <div className="col-span-6 row-span-9 md:col-span-2 lg:col-span-1 border-r border-gray-300">Error: {error}</div>;
    }

    return (
        <div className="col-span-6 row-span-9 md:col-span-2 lg:col-span-1 border-r border-gray-300">
            <div className="sidebar">
                {groups.map((group: GroupResponseDto) => (
                    <Group key={group.id} name={group.name} onSelectGroupId={onSelectGroupId} groupId={group.id} />
                ))}
            </div>
        </div>
    );
}

function Group({ name, onSelectGroupId, groupId }: { name: string | undefined, onSelectGroupId: (groupId: string) => void, groupId?: string }) {
    return (
        <div
            className="group-item p-2 border-b border-gray-200 hover:bg-gray-100 cursor-pointer"
            onClick={() => { if (groupId) onSelectGroupId(groupId); }}
        >
            {name || 'Unnamed Group'}
        </div>
    );
}