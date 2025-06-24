import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  CircularProgress,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { getAssignments } from '../services/api';
import { TaskAssignment } from '../types';

const Dashboard: React.FC = () => {
  const { data: assignments, isLoading } = useQuery({
    queryKey: ['assignments'],
    queryFn: getAssignments,
  });

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Current Assignments</Typography>
      </Box>

      <Grid container spacing={3}>
        {assignments?.map((assignment: TaskAssignment) => (
          <Grid item xs={12} sm={6} md={4} key={assignment.id}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  {assignment.task.taskName}
                </Typography>
                <Typography color="textSecondary">
                  Assigned to: {assignment.member.host}
                </Typography>
                <Typography color="textSecondary">
                  Slack: {assignment.member.slackId}
                </Typography>
                <Typography color="textSecondary">
                  Rotation Rule: {assignment.task.rotationRule}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default Dashboard; 